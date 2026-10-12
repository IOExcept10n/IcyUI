# Typed markup bindings: `x:DataType`, static and compiled paths

> Design spec, discussed with Ivan on 2026-10-12, following the review of the concepts docs (point 3).

## Context

IcyUI has three property-path kinds, but markup can create only one:

- `DynamicPropertyPath` resolves members on the source's runtime type at every read. `{Binding}` always builds it
  (`BindingExtension.cs`), so a misspelled path only logs a warning (2001) when the binding first reads, and reads
  `null`.
- `PropertyPath` is typed: it resolves its members against a known type once, when it's built, and throws for a member
  that doesn't exist.
- `CompiledPropertyPath` is a `PropertyPath` whose getter and setter are compiled expression trees: faster reads, a
  noticeable one-time cost to compile.

`x:DataType` is already a directive: the loader resolves it (so a misspelled type fails at load) and stores it in
`MarkupLoadContext.DataTypes`, with no runtime effect. Its remarks say it's waiting for bindings and a future source
generator.

## Decisions

| Decision | Choice |
|---|---|
| Syntax | `x:DataType="vm:MainMenuViewModel"`: a type name resolved like `Style TargetType`. |
| Default path kind | **Static** (`PropertyPath`) for a `{Binding}` whose source type is known; **dynamic** otherwise, as today. |
| Compiled | Opt-in per binding: `{Binding Score, Compiled=True}`. Valid only where the source type is known; otherwise a load error. |
| Source type | `x:DataType` for `DataContext` bindings. `ElementName=`: the named element's type. `Source=`: the object's type. All three are checked at load. |
| Scope of `x:DataType` | Inherited down the element tree. A template (`DataTemplate`, `ControlTemplate`) is a boundary: its content sees only an `x:DataType` declared on the template or inside it. |
| Wrong `DataContext` at run time | Static: warn once (new event 2002) and fall back to a dynamic path for that binding. Compiled: throw, since the user chose it on purpose. |
| Compiled cost | One compiled path per (registry, type, path), shared by every binding on it. |
| Source generator | Out of scope (post-MVP). `x:DataType` is what it will read. |

## Design

### 1. Finding the source type

The loader gives `MarkupExtensionContext` a new property:

```csharp
/// The x:DataType in effect for the element being loaded, or null when none is declared.
public Type? DataType { get; }
```

It's the `x:DataType` of the nearest element, the target included, walking up the `XElement` ancestors and stopping at a
template boundary (an element whose type is `DataTemplate` or `ControlTemplate`; the template element itself still
counts, so `<DataTemplate x:DataType="vm:Item">` works).

Two consequences for the loader:

- **`x:DataType` is read before the element's other attributes**, so a `{Binding}` on the same element sees it. Today
  directives are applied in attribute order.
- **Templates are loaded later, with a fresh `MarkupLoadContext`**, from the captured `XElement` content. Walking the
  `XElement` ancestors (not the context's dictionary) is what makes the lookup work there; the resolved types are
  cached per `XElement` in a table owned by the loader, so a template built for 1,000 items resolves once.

### 2. `BindingExtension`

New property:

```csharp
/// Whether the path is compiled (CompiledPropertyPath) rather than resolved through reflection. Requires a known
/// source type: x:DataType for DataContext bindings, or ElementName / Source.
public bool Compiled { get; set; }
```

`ProvideValue` picks the source type, then the path:

| Binding | Source type | Path |
|---|---|---|
| `Source=` set | `Source.GetType()` | static, or compiled with `Compiled=True` |
| `ElementName=` set | the named element's `GetType()` | static, or compiled |
| neither, `context.DataType` set | `context.DataType` | static, or compiled, wrapped in a `CheckedPropertyPath` (§3) |
| neither, no `x:DataType` | unknown | dynamic, as today; `Compiled=True` is a `MarkupException` ("needs x:DataType") |

A path that doesn't resolve against the source type is a `MarkupException` at the attribute, with the member list as a
`NameSuggestion` ("'Scroe' isn't a property of 'MainMenuViewModel'. Did you mean 'Score'?"). `{Binding}` with no
path (the source itself) needs no path type and stays as today.

### 3. Run-time type check: `CheckedPropertyPath`

A `DataContext` binding's source changes at run time, so its type can differ from `x:DataType`. A new internal
`IPropertyPath`, `CheckedPropertyPath`, wraps the typed path:

- When the source is an instance of the declared type (subclasses and implementations included), it delegates to the
  typed path.
- **Static:** otherwise, it logs `IcyLog.BindingSourceTypeMismatch` (2002, Warning, once per binding: "Binding path
  'Score' expects a 'MainMenuViewModel' (x:DataType) but its data context is a 'PauseViewModel'; using a dynamic
  path.") and from then on delegates to a `DynamicPropertyPath` for that binding.
- **Compiled:** otherwise, it throws `BindingException` naming both types. The exception surfaces from whatever set
  the data context (a `DataContext` assignment, an `ItemsSource` item), which is where the mistake is.
- A `null` source isn't a mismatch: nothing is read.

The check is one type test per read. `Binding` itself is unchanged; it only sees an `IPropertyPath`.

### 4. The compiled-path cache

`ReflectionConfiguration` gets an internal cache: `ConcurrentDictionary<(PropertyRegistry, Type, string), PropertyPath>`
for both static and compiled paths. Both are immutable after construction, so bindings share them. Loads are
single-threaded in practice; the concurrent dictionary keeps a background `LoadAsync` safe. Paths are never evicted:
their number is bounded by the distinct paths in the game's markup.

### 5. What doesn't change

- `{TemplateBinding}` already binds to a known control property; it's untouched.
- Bindings created in code choose their path themselves, as today.
- Without `x:DataType`, markup behaves exactly as now.

## Testing

- Load-time errors: a misspelled member under `x:DataType`, under `ElementName=`, under `Source=`; `Compiled=True`
  without a source type. Each names the file, line and column.
- Path kinds: the binding gets a `PropertyPath`, a `CompiledPropertyPath`, or a `DynamicPropertyPath` per the table in §2.
- Scope: `x:DataType` on an ancestor, on the target itself, overridden by a nested one, not leaking into a
  `DataTemplate`, declared on a `DataTemplate`.
- Run time: a subclass `DataContext` works; a wrong type warns once and still reads through the dynamic fallback
  (static); a wrong type throws (compiled); `null` does neither.
- The cache: two bindings on the same (type, path) share one compiled path; a different registry gets its own.
- Two-way and `OneWayToSource` through static and compiled paths, including text conversion (#5, the culture work).
- The docs snippets with `x:DataType` load and bind (`DocsMarkupSnippetTests`).

## Performance

- **Load:** a static path costs one reflection lookup per segment, once per (type, path) thanks to the cache; less than
  the dynamic path's first read. A compiled path costs an expression compile (tens to hundreds of microseconds) once
  per (type, path). A screen with 50 distinct compiled paths adds a few milliseconds to its first load, none after.
- **Per read:** static reads through cached `PropertyInfo`s; compiled reads through a delegate; both skip the dynamic
  path's per-read member lookup. The `x:DataType` check adds one type test.
- **NativeAOT:** `Expression.Compile` falls back to the interpreter: compiled paths still work, at roughly static speed.
  The docs say so.
- No per-frame work is added; nothing changes for markup without `x:DataType`.

## Docs

- Data binding concept page: a "Binding kinds" section (dynamic, static, compiled; how to choose; the AOT note) and the
  run-time type rule.
- Markup reference: `x:DataType` (scope, template boundary) and `{Binding ... Compiled=True}`.
- `MarkupDirectives.DataType` remarks: it now has a run-time effect.

## Out of scope

- The source generator that turns compiled paths into real code (post-MVP). It will read `x:DataType` and the binding
  paths from the `.xml` files.
- Editor support (validating paths as you type, autocomplete); `x:DataType` is the input it will need.
- Typed `{TemplateBinding}` changes.
