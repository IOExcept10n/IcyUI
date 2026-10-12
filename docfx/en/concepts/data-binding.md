# Data binding

A binding connects a property of an element to a property of your data, so the screen shows your game's state and the
player's input changes it, without code that copies values back and forth. You write the data as a *view model*, a
plain C# class, and the markup reads from it.

## The data context

Every element has a <xref:Icy.UI.UIElement.DataContext>: the object its bindings read from. It's inherited, so setting
it on a menu's root (`LoadMarkup("Menu.xml", viewModel)` does) gives the whole menu the same view model. A binding's
`Path` names a property of that object: `{Binding Title}`, `{Binding Player.Name}` for a nested one, or
`{Binding Levels[0]}` for an indexer.

For the screen to follow changes, the view model must raise `PropertyChanged`. Deriving from CommunityToolkit.Mvvm's
`ObservableObject` (IcyUI already depends on it) and setting properties with `SetProperty` does that. A view model that
doesn't is read once, when the binding starts.

## Modes

[!code-xml[](../../snippets/markup/concepts/binding.xml)]

| Mode | Direction | When |
|---|---|---|
| `OneWay` | view model → element | The default for display properties: text, colors, sizes. |
| `TwoWay` | both ways | The default for the properties the player edits: a text box's text, a slider's value, a check box, a list's selection, an expander's state. |
| `OneTime` | view model → element, once | Values that never change after the screen opens. |
| `OneWayToSource` | element → view model | Reporting an element's state without showing yours. |

Write `Mode=` only to change the default: `{Binding Title, Mode=OneTime}`.

## Binding to another element

`ElementName` binds to a named element (`x:Name`) instead of the data context. In the example, the label shows the
slider's value as it moves. `Source` binds to a specific object instead; markup can't nest `{StaticResource}` inside
`{Binding}` yet, so set such a binding's source from code.

## Conversion

A binding converts between the two property types when they differ:

- numbers, enums and other values shown in a text property are formatted as text;
- text bound into a number parses in the UI's culture, and numbers shown as text are formatted in it (`3,5` in
  Russian); a two-way text box bound to a number keeps the old value while the text doesn't parse and shows its
  `Invalid` state (a red border in the default theme);
- typed numbers take either `.` or `,` as the decimal point, whatever the language, since the player's keyboard may not
  match it, and no thousands separators: `1,5` and `1.5` are both 1.5, and `1,000` is 1. Whole-number types take
  neither;
- a string bound to a `Content` property becomes a `TextBlock`.

Custom conversion uses a `TypeConverter` from code (`Binding.ConverterParameters`); markup has no `Converter=`.

The culture is the configuration's <xref:Icy.Configuration.IcyConfiguration.Culture>. By default it follows the
game's UI language (`CultureInfo.CurrentUICulture`), not the player's regional settings, so numbers look the way the
game's language writes them. Set it with `WithCulture` when the game picks its language itself; existing bindings use
the new culture the next time they update. One binding can use its own with `Binding.Culture`. Markup attributes are
always read in the invariant culture: `Width="3.5"` means the same in every language.

## Binding kinds: dynamic, static, compiled

A binding reads its path one of three ways:

| Kind | When | Misspelled path | Reads |
|---|---|---|---|
| Dynamic | The source type isn't known: no `x:DataType`, no `ElementName` or `Source`. | A warning (event 2001) on first read, and `null`. | Looks the members up on each read. |
| Static | The source type is known. The default then. | The page fails to load, naming the line. | Through members found once. |
| Compiled | `Compiled=True`, with a known source type. | The page fails to load. | Through compiled code: the fastest. |

Declare the data context's type with `x:DataType`; every `{Binding}` below it is static, and `Compiled=True` makes one
compiled:

[!code-xml[](../../snippets/markup/concepts/typed-binding.xml)]

- `x:DataType` applies to the element and everything inside it, except templates: a `DataTemplate` declares the type of
  its items with an `x:DataType` of its own.
- `ElementName=` and `Source=` bindings are static without `x:DataType`: their source is known when the page loads.
- If the data context turns out to be another type at run time, a static binding logs a warning (event 2002) and keeps
  working dynamically; a compiled one throws, since you asked for it on purpose. A subclass of the declared type is
  fine.
- Compiling a path costs some time the first time a page uses it; every binding on the same path shares it. In a
  NativeAOT build, compiled paths are interpreted and run at about the speed of static ones.

## Updates every frame

`UpdateTargetTrigger=EveryFrame` re-reads the source each frame instead of waiting for `PropertyChanged`. Use it for
values your game changes constantly and doesn't notify, such as a position or a timer; it costs a property read per
frame.

## Pitfalls

- **A typo in a path doesn't throw.** The element shows nothing, and IcyUI logs a warning (event 2001) naming the
  member and the type it was looked up on; see [Logging and diagnostics](logging-and-diagnostics.md).
- **Setting a bound property in code replaces the binding's value only until the source changes.** For a display
  property, change the view model instead.
- **A binding on an item template binds to the item,** not to the list's data context.

## See also

- [Your first screen](../getting-started/first-screen.md): a menu bound to a view model.
- [The data context](../markup/index.md#the-data-context) in the markup reference.
- [Lists and selection](../controls/lists.md): `ItemsSource` and `SelectedItem`.
