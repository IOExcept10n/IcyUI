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
- text bound into a number parses in the UI's culture (`"3,5"` is 3.5 in Russian), and numbers shown as text are
  formatted in it; a two-way text box bound to a number keeps the old value while the text doesn't parse and shows its
  `Invalid` state (a red border in the default theme);
- a string bound to a `Content` property becomes a `TextBlock`.

Custom conversion uses a `TypeConverter` from code (`Binding.ConverterParameters`); markup has no `Converter=`.

The culture is the configuration's <xref:Icy.Configuration.IcyConfiguration.Culture>. By default it follows the
game's UI language (`CultureInfo.CurrentUICulture`), not the player's regional settings, so numbers look the way the
game's language writes them. Set it with `WithCulture` when the game picks its language itself; existing bindings use
the new culture the next time they update. One binding can use its own with `Binding.Culture`. Markup attributes are
always read in the invariant culture: `Width="3.5"` means the same in every language.

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
