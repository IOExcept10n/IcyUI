# Localization: `{Localize}`, localizers and language switching

> Design spec, discussed with Ivan on 2026-10-12, following the review of the concepts docs (point 4). It brings forward
> the "library localization" item the docs spec (`2026-10-12-docs-plans-2-3-design.md`) had deferred to alpha.2.

## Context

The old iteration (`main`) resolved localized strings in markup through `LocalizationResolver`. The rewrite kept only the
plumbing:

- `ILocalizer` (`Icy.Assets`) has `TargetCulture` and `string Localize(FormattableString value)`. It's meant to hide
  where strings come from: localization files, RESX resources, gettext, or a plain dictionary.
- Its one implementation, `ResourceLocalizer`, reads `AssetConfiguration.SharedResources`: a single flat dictionary
  with one fixed culture (`ResourceRegistry.PreferredCulture`). A missing key throws `KeyNotFoundException`, although
  the interface promises the key back.
- `AssetConfiguration.Localizer` holds it, settable with `WithLocalizer`. Nothing calls `Localize`, and markup has no way
  to ask for a string.

`IcyConfiguration.Culture` (added with this review's point 2) is the culture numbers are formatted and parsed in. It
follows `CultureInfo.CurrentUICulture` until a game sets it.

## Decisions

| Decision | Choice |
|---|---|
| Interface | `ILocalizer.Localize(string key, CultureInfo culture)` returns the string, or `null` when the localizer has none. It stays the extension point for any source. |
| Built-in localizers | `DictionaryLocalizer` (code), `ResourceManagerLocalizer` (RESX), `PoLocalizer` (gettext `.po` files), and `CompositeLocalizer` to chain them. |
| Markup | `{Localize Menu.Play}` resolves once, at load. No per-frame work and no subscriptions on the target. |
| Language | `IcyConfiguration.Culture`, the same setting as number formatting: one switch changes both. |
| Switching | Setting `Culture` raises `CultureChanged`; IcyUI re-applies every localized property. Bindings pick up the number format on their next update. |
| Code-built UI | `element.Localize(nameof(TextBlock.Text), "Menu.Play")`, re-applied on a switch like markup. |
| Missing key | Shows the key, and logs a warning once per key and culture (new range 4xxx, event 4001). |
| Arguments (`"Score: {0}"`) | Out of scope; a later release. |
| `ResourceRegistry` | Left as it is, minus `PreferredCulture`; it's no longer the localization source. `ResourceLocalizer` is removed. |

## Design

### 1. `ILocalizer` and the namespace

The localization types move to a new namespace, `Icy.Localization`:

```csharp
public interface ILocalizer
{
    /// The string for key in culture, or null when this localizer has none: the caller decides what to show.
    /// Implementations try the culture's parents themselves (ru-RU, then ru) when their source supports it.
    string? Localize(string key, CultureInfo culture);
}
```

`TargetCulture` goes: the culture is an argument, so one localizer serves every language.

### 2. Built-in localizers

**`DictionaryLocalizer`**: strings added from code, per culture: `Add(CultureInfo culture, string key, string value)`
and a collection initializer. Lookup tries the culture, its parents, then `CultureInfo.InvariantCulture` (the strings
added without a culture). Also the test double.

**`ResourceManagerLocalizer(ResourceManager resources)`**: `resources.GetString(key, culture)`. `ResourceManager`
already falls back through satellite assemblies to the neutral resources. Compiled into the build; no files to ship.

**`PoLocalizer`**: gettext `.po` files, one per language, loaded through the asset pipeline.

- `new PoLocalizer(AssetConfiguration assets, string folder, CultureInfo defaultCulture)`. For a culture it loads
  `{folder}/{name}.po` for `ru-RU`, then `ru`, then the default culture, lazily: each file is read once, the first time
  its culture is asked for, and cached. A file that doesn't exist is remembered as missing.
- The reader is IcyUI's own (no NuGet dependency), as a new `IAssetImporter<PoCatalog>` with `.po` mapped to
  `text/x-gettext-translation`. It reads `msgctxt`, `msgid`, `msgstr`, multi-line strings, the C escapes (`\n`, `\t`,
  `\"`, `\\`) and `#` comments. It skips the header entry (`msgid ""`), entries flagged `#, fuzzy` (gettext's own
  rule), and empty `msgstr`s (untranslated). A plural entry contributes its `msgstr[0]`; plural forms come with
  arguments, later.
- Keys are `msgid`s, so a game can use keys (`msgid "Menu.Play"`) or the source text itself, gettext-style. An entry
  with a context is found by `{Localize Play, Context=Menu}`, keyed `"Menu\u0004Play"` as gettext does.
- Malformed input is a `FormatException` naming the file and line.

**`CompositeLocalizer(params ILocalizer[] localizers)`**: asks each in order; the first non-`null` string wins. A game
can put mod files before its own, or a `DictionaryLocalizer` last as a safety net.

The default `AssetConfiguration.Localizer` is an empty `DictionaryLocalizer`.

### 3. `IcyConfiguration`

- `CultureChanged` (`EventHandler`) is raised when `Culture` is set to a different culture. While `Culture` only
  follows `CurrentUICulture`, nothing is raised: a game switches language by setting `Culture`.
- `string Localize(string key, string? context = null)`: the localizer's string for the current culture, or the key
  (with the 4001 warning) when there's none. For code that formats strings itself, such as view models.

### 4. Localized properties: `{Localize}` and `Localize(...)`

`LocalizeExtension` (`{Localize Key}`, default property `Key`; optional `Context`):

- Valid on any registered property that takes a `string`, or `object` (so `Content="{Localize Menu.Play}"` becomes a
  `TextBlock` through the configuration's text converter, as a plain string attribute does).
- `ProvideValue` returns the string, which the loader assigns like any attribute value, and registers the target in the
  configuration's `LocalizedProperties` registry.
- On a style setter (the target isn't an element), the string resolves once at load and isn't re-applied on a switch:
  styles are shared, so re-applying would mean re-styling every element. Documented as a limitation; templates are fine,
  since their content is built per control with the control's elements as targets.

`UIElementExtensions.Localize(this UIElement element, string propertyName, string key, string? context = null)` does
the same from code: assigns the string now and registers it.

**The registry** (internal, one per `IcyConfiguration`) is a list of `(WeakReference<object> target, IPropertyReference
property, string key, string? context, string lastValue)`. On `CultureChanged` it walks the list once:

- a dead target is removed;
- if the property's current value is no longer `lastValue`, the game has replaced the text since, so the entry is
  removed and the value left alone;
- otherwise the property gets the string for the new culture, and `lastValue` is updated.

It never keeps an element alive, and does nothing between switches.

### 5. Removed and changed API

| API | Change |
|---|---|
| `Icy.Assets.ILocalizer` | Moved to `Icy.Localization`; `Localize(string, CultureInfo)`; `TargetCulture` removed. |
| `ResourceLocalizer` | Removed. |
| `IResourceRegistry.PreferredCulture`, `ResourceRegistry(CultureInfo?)` | Removed. |
| `WithLocalizer(...)` | Kept. New helper: `UsePoLocalization(string folder, CultureInfo defaultCulture)`. |

### 6. Engines

Nothing engine-specific: `.po` files load through the asset pipeline, which both engines already feed. FNA gets it with
the core.

## Samples

A `LocalizationDemo` in `Shared Samples`, registered in both hosts' `SampleGame.cs`: a small menu in markup with
`{Localize}` texts and a number field, `en.po` and `ru.po` next to it, and a language switch that shows the strings and
the number format changing together.

## Testing

- `.po` reader: entries, contexts, multi-line strings, escapes, comments, header, fuzzy and empty entries, plural
  entries, malformed input with file and line.
- Localizers: culture fallback (`ru-RU` → `ru` → default) for the dictionary and `.po` localizers; lazy, once-per-file
  loading; a missing file; `ResourceManagerLocalizer` against a test `.resx`; composite order.
- `{Localize}`: on a `string` property, on `Content`, with `Context`, in a template, in a style setter (resolved once).
- Switching: markup and code-registered properties change; a property the game overwrote since isn't touched; a
  collected element is dropped; nothing happens when the culture is set to the same value.
- Missing key: the key is shown and 4001 is logged once per key and culture.
- Docs snippets: the localization markup snippet loads and switches (`DocsMarkupSnippetTests`).

## Performance

- **Load:** one localizer lookup per `{Localize}` (dictionary lookups), plus one registry entry. Each `.po` file is
  parsed once, the first time its culture is needed: a few milliseconds for thousands of entries.
- **Switch:** one pass over the registry, one lookup and one property set per localized property. Layout re-runs for
  the texts that changed, as for any text change.
- **Per frame:** nothing.
- **Memory:** one small entry per localized property, plus each loaded language's strings.

## Docs

- A Localization concept page (en, with a Russian stub): the localizers and how to choose, `.po` files and tools,
  `{Localize}`, switching languages, code-built UI, missing keys, limitations.
- Markup reference: `{Localize}`.
- Configuration and data binding pages: one `Culture` for strings and numbers.

## Out of scope

- Arguments and plural forms (`"{0} coins"`).
- Right-to-left text and per-language fonts (a font fallback per culture).
- Re-applying `{Localize}` inside style setters.
- Binary `.mo` files.
