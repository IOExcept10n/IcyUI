# Markup reference

IcyUI markup is an XML dialect for describing UI trees. It's deliberately familiar to anyone who knows WPF or
Avalonia XAML, with the common cases simplified: no namespace declarations for built-in controls, and plain text where
a control expects content.

```xml
<StackPanel Orientation="Vertical" Padding="24">
  <TextBlock FontSize="28">Hello</TextBlock>
  <Button x:Name="ok" Padding="12,6">Click Me</Button>

  <Grid>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*"/>
      <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    <TextBlock Grid.Column="1">Cell</TextBlock>
  </Grid>
</StackPanel>
```

Markup is loaded with <xref:Icy.Markup.MarkupLoader> (see [Your first screen](../getting-started/first-screen.md)), or
through the asset pipeline with <xref:Icy.Markup.MarkupImporter>. This page describes the language itself.

## Namespaces

| Form | Meaning |
|---|---|
| *(none)* | Built-in IcyUI controls. The default namespace is implicit. |
| `xmlns="https://icyui.dev/markup"` | The same built-in namespace, stated explicitly. |
| `xmlns:x="https://icyui.dev/markup/x"` | The directive namespace. It's bound to the `x` prefix implicitly unless you rebind it. |
| `xmlns:game="clr-namespace:MyGame.UI;assembly=MyGame"` | A CLR namespace in a named assembly. |
| `xmlns:game="clr-namespace:MyGame.UI"` | A CLR namespace in the resolver's default assembly. |

`https://icyui.dev/markup`, the address of this page, is the identifier of IcyUI's markup namespace.

The `clr-namespace:` form works as in WPF. When `assembly=` is omitted, types are looked up in
<xref:Icy.Data.IAssemblyResolver.DefaultAssembly>.

### How an element name becomes a type

For an element `<Name>` or `<prefix:Name>`:

1. **Prefixed:** the prefix's namespace is resolved, then `Namespace.Name` is looked up in the declared assembly.
2. **Unprefixed:** the built-in IcyUI controls are searched first, then the *short-name registry*.
3. If nothing matches, loading fails with a <xref:Icy.Markup.MarkupException> naming the tag, with a suggestion
   when a similarly named type exists.

Built-in controls always win over the short-name registry, and registering a short name that collides with a built-in
control is an error at registration time. To use your own type in place of a built-in one, give it an explicit prefix.

### Short names for your own controls

Controls your game uses constantly can be registered under a short name so they need no prefix:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Markup.cs#short-names)]

The `clr-namespace:` form always works without registration; short names only remove prefixes.

## Attributes

Each attribute is resolved in this order, and the first match wins:

1. **Namespace declaration** (`xmlns`, `xmlns:*`): consumed by the parser.
2. **Directive** in the `x:` namespace (see [Directives](#directives)).
3. **Attached property:** the name contains a `.` (see [Attached properties](#attached-properties)).
4. **Property** of the element's type: a registered IcyUI property first, then any public settable CLR property.
5. Otherwise, loading fails. **Unknown attributes are errors**, never silently ignored, so a typo can't turn into a
   no-op. An attribute naming an **event** is an error too: wiring event handlers from markup isn't supported yet (use
   commands, or subscribe in code).

Registered properties are preferred because values written through them take part in the same precedence system as
styles, visual states and animations. The CLR fallback covers what isn't registered, such as collections that are
filled rather than assigned (`Panel.Children`) and plain data objects (`ColumnDefinition.Width`).

### Values

- A value that's a [markup extension](#markup-extensions) is evaluated, and its result is used.
- Any other value is converted from the string to the property's type, so `Padding="12,6"`, `Background="#FF3C64C8"`
  and `Orientation="Vertical"` all work.

Values set in markup are **local values**, exactly like values set from code: they win over styles and animations.

## Directives

| Directive | Valid on | Meaning |
|---|---|---|
| `x:Name` | any `UIElement` | Sets the element's `Name` and registers it in the document's name scope, so code can find it with `FindControl<T>(name)`. Names must be unique within a document. |
| `x:Class` | the root element | The type to instantiate instead of the tag's type (see [Backing classes](#backing-classes-xclass)). |
| `x:Key` | an entry of a dictionary | The key the entry is stored under, for example in `Resources`. |
| `x:DataType` | any element | Declares the type the subtree's data context is expected to have. `{Binding}` paths below it are checked at load time and read through members found once; a misspelled type or path fails the load. It applies to the element and everything inside it, except templates (`DataTemplate`, `ControlTemplate`): a `DataTemplate` declares the type of its items with an `x:DataType` of its own. See [Binding kinds](../concepts/data-binding.md#binding-kinds-dynamic-static-compiled). |

## Markup extensions

An attribute value is a markup extension when it starts with `{` and ends with `}`. A leading space makes the value
plain text.

```
extension := '{' name ( WS arguments )? '}'
arguments := positional ( ',' named )*  |  named ( ',' named )*
named     := name '=' value
```

```xml
<TextBlock Text="{Binding UserName}"/>
<TextBlock Text="{Binding Path=UserName, Mode=OneWay}"/>
<Button Command="{Binding SubmitCommand}"/>
```

The first argument may be positional; each extension defines what it means. A value may be single-quoted to contain a
comma or a brace: `{Binding Path='A, B'}`.

To write a literal value that starts with `{`, prefix it with an empty pair of braces, as in XAML:

```xml
<TextBlock Text="{}{not an extension}"/>
```

### Built-in extensions

| Extension | Purpose |
|---|---|
| `{Binding ...}` | Binds the property to data. Arguments: `Path` (positional), `Source`, `ElementName`, `Mode`, `UpdateTargetTrigger`, `Compiled` (`True` for a compiled path; needs a known source type). See [Binding kinds](../concepts/data-binding.md#binding-kinds-dynamic-static-compiled) and <xref:Icy.Markup.Extensions.BindingExtension>. |
| `{StaticResource Key}` | Looks a resource up by key, searching the `Resources` of the element under construction and then its ancestors up to the document root. Resolved once, at load time. See <xref:Icy.Markup.Extensions.StaticResourceExtension>. |
| `{TemplateBinding Path}` | Inside a control template, binds to a property of the templated control. See <xref:Icy.Markup.Extensions.TemplateBindingExtension>. |

Binding a property that can't carry bindings (a non-bindable or unregistered property) is an error, not a warning: the
binding would otherwise be dropped silently.

### Your own extensions

Implement <xref:Icy.Markup.IMarkupExtension> and register it with
<xref:Icy.Markup.MarkupConfiguration.RegisterExtension*>. `{MyThing}` then maps to your type. The name defaults to the
type name without an `Extension` suffix. <xref:Icy.Markup.MarkupExtensionContext> gives the extension the target
element and member, the name scope, the configuration and the source position. Returning
<xref:Icy.Markup.MarkupValue.Unset> tells the loader that the extension assigned the property itself (this is how
`{Binding}` installs a binding).

### The data context

A `{Binding}` without `Source` or `ElementName` reads from <xref:Icy.UI.UIElement.DataContext>. The data context is
**inherited**: an element without one of its own uses its nearest ancestor's. Bindings keep following the inherited
value, so setting `DataContext` on a root after loading it, as [Your first screen](../getting-started/first-screen.md)
does, updates every binding below it.

## Attached properties

Attached properties are written as `Owner.Property` on the element they apply to:

```xml
<TextBlock Grid.Column="1" Grid.Row="0">Cell</TextBlock>
<Widget game:Dock.Side="Left"/>
```

The part before the last `.` is resolved as a type, exactly like an element name, and the property is looked up on
**that** type: `Grid.Row` belongs to `Grid`, not to `TextBlock`.

## Content, children and property elements

### Content property

A type marks one property as its content with <xref:Icy.Markup.ContentPropertyAttribute>. Child elements and text
are assigned to it.

| Type | Content property | Kind |
|---|---|---|
| `Panel` (`StackPanel`, `Grid`, ...) | `Children` | a collection: each child is **added** |
| `Border` | `Child` | a single value: **assigned** |
| `ContentControl` (`Button`, `Window`, ...) | `Content` | a single value: **assigned** |
| `TextBlock` | `Text` | a string |

A second child for a single-valued content property is an error.

### Property elements

A property whose value can't be written as a string is set with a property element, `<Type.Property>`:

```xml
<Grid>
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="*"/>
  </Grid.ColumnDefinitions>
</Grid>
```

The property's current value decides what happens: for a list, each child is added; for a dictionary, each child is
added under its `x:Key`; otherwise the single child is assigned.

### Text content

When an element has text and no child elements, the text becomes the content property's value:

```xml
<TextBlock FontSize="28">Hello</TextBlock>
<Button>Click Me</Button>
```

For `Button`, whose content is an element rather than a string, the text is wrapped in a `TextBlock` automatically.
The same happens to a string assigned as an attribute or through a binding, anywhere an element is expected:

```xml
<Button Content="Play"/>
<Button Content="{Binding Title}"/>
```

A binding creates a new `TextBlock` each time its value changes; for text that changes often, bind the text itself:
`<Button><TextBlock Text="{Binding Score}"/></Button>`. Text without a `FontFamily` uses the configuration's default
family (Segoe UI with the engine defaults); when no font resolves, IcyUI logs a warning.

## Resources and styles

Every element has a `Resources` dictionary. Entries are declared as property elements and found by
`{StaticResource}` from the element itself and everything inside it. Declare `Resources` before the children that use
them: a `{StaticResource}` is resolved while the document loads, so it only sees resources that are already declared.

[!code-xml[](../../snippets/markup/resources-and-styles.xml)]

- A `Style` sets properties on every control it applies to. Its attributes are the property values, as with any other
  element.
- A `Style` **without** `x:Key` is *implicit*: it applies to every control in the subtree whose type is exactly its
  `TargetType` (not a derived type) and that has no `Style` of its own.
- A `Style` **with** `x:Key` applies only where `Style="{StaticResource Key}"` names it.
- `Style.StateGroups` declares visual states, such as a hover color with a transition.
- A standalone `<ResourceDictionary>` document can merge other dictionaries with
  `<ResourceDictionary.MergedDictionaries>`, including files loaded through the asset pipeline with `Source="..."`.

The built-in theme is such a dictionary. Load it with `UseDefaultTheme()` (see <xref:Icy.Configuration>).

## Backing classes (`x:Class`)

A markup document can name the type it produces:

```xml
<Page x:Class="MyGame.UI.MainMenuPage">
  <StackPanel>...</StackPanel>
</Page>
```

The loader checks that the named type derives from the tag's type and instantiates it instead. Without `x:Class`, the
tag's own type is created. This works for any element, not only pages.

### Pages and navigation

A <xref:Icy.UI.Controls.Page> is a content control whose content is the page's UI. A <xref:Icy.UI.Controls.Frame>
hosts pages and owns a navigation service (`Frame.Navigation`):

- `NavigateTo(page)` shows a page, calling its `OnNavigatedFrom`/`OnNavigatedTo`.
- `Navigate(path)` first loads the page's markup through the asset pipeline. It returns `null` for a path that doesn't
  exist, rather than throwing.
- Back and forward history keeps a page only when its `KeepAlive` is set. Navigating to a new page clears the forward
  history, as in a browser.
- `Page.Initialize()` runs once per page instance. `Page.Prepare()` runs on every activation, including returns to a
  kept-alive page.

## Errors

Every markup error is a <xref:Icy.Markup.MarkupException> carrying the source path, line and column:

```
MyGame/UI/MainMenu.xml(14,6): Unknown property 'Bakground' on 'Button'. Did you mean 'Background'?
```

- Loading stops at the **first** error; no partial tree is returned.
- Messages name the file, position, offending symbol and containing type.
- Unknown property and type names come with a near-match suggestion.

## Schemas

Two XML schemas describe the fixed parts of the language, for editors that offer completion and validation from XSD:

- [`icyui-markup.xsd`](icyui-markup.xsd): the document shape in the built-in namespace.
- [`icyui-markup-x.xsd`](icyui-markup-x.xsd): the `x:` directives.

> [!NOTE]
> These schemas are skeletons: they cover the namespaces and the directives, not every control and property. A
> schema generated from the available controls isn't available yet.

## Not yet supported

> [!NOTE]
> - Event handlers in markup (`Click="OnPlay"`): use commands, or subscribe to events in code.
> - Nested markup extensions, such as `{Binding Source={StaticResource Data}}`.
> - Inherited text properties: `FontFamily`, `FontSize` and `Foreground` don't flow from a parent to its children.
>   Set them on each element, or with an implicit `Style`.
> - Declaring new properties in markup, for components defined entirely in markup.
> - A source generator for `x:Class` documents (typed fields for `x:Name` elements).
