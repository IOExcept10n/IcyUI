# Buttons

![A Play button, a checked Music toggle button and a checked Subtitles check box](../../images/controls/buttons.png)

A <xref:Icy.UI.Controls.Button> runs an action when the player clicks or taps it, or activates it with the keyboard or a
gamepad. A <xref:Icy.UI.Controls.ToggleButton> and a <xref:Icy.UI.Controls.CheckBox> also keep an on/off state: use a
toggle button for a mode the player switches (music on or off), and a check box for an option in a list of settings.

## Markup

[!code-xml[](../../snippets/markup/controls/buttons.xml)]

Text inside a button becomes its content: a string is shown in a <xref:Icy.UI.Controls.TextBlock>. Any element can be
the content instead, for example an icon and a label in a `StackPanel`.

## Key properties

| Property | Description |
|---|---|
| <xref:Icy.UI.Controls.Button.Command> | The command the button runs. Bind it to your view model's command. |
| <xref:Icy.UI.Controls.ContentControl.Content> | What the button shows: text or any element. |
| <xref:Icy.UI.Controls.ToggleButton.IsChecked> | Whether a toggle button or check box is on. |
| <xref:Icy.UI.Controls.CheckBox.CheckBrush> | The color of a check box's check mark. |

## Events and commands

A button raises <xref:Icy.UI.Controls.Button.Click> and runs its <xref:Icy.UI.Controls.Button.Command> when activated. A toggle button and a check box flip
<xref:Icy.UI.Controls.ToggleButton.IsChecked> and raise <xref:Icy.UI.Controls.ToggleButton.IsCheckedChanged>.

## Styling

In the default theme, buttons are dark grey with light text, and they size to their content and sit centered in the
space they're given. Set `HorizontalAlignment="Stretch"` (or `VerticalAlignment`) on a button to make it fill its slot,
for example in a menu column.

The theme's styles use these visual state groups. A group of your own with the same name replaces the theme's (see
[Control templates and visual states](../concepts/templates-and-visual-states.md#what-names-are-for)):

| Control | `CommonStates` | `FocusStates` | `CheckStates` |
|---|---|---|---|
| `Button` | `Hovered`, `Pressed` | `Focused` | |
| `ToggleButton` | `Hovered`, `Pressed` | `Focused` | `Checked` |
| `CheckBox` | `Hovered` | `Focused` | `Checked` |

See [Resources and styles](../markup/index.md#resources-and-styles) in the markup reference.

## Keyboard, gamepad and touch

A focused button runs its command on Enter or the gamepad's A button; a toggle button or check box flips
its state. Arrow keys and the D-pad move focus between controls. A tap runs the command, like a click.

## ToggleButton

Looks like a button and stays highlighted while it's on (the `Checked` state).

## CheckBox

A box with a check mark next to its content. Use it in settings lists; use a toggle button for a mode on a toolbar or
HUD.

## See also

- [Text](text.md) for the text inside buttons.
- [Panels](panels.md) to arrange buttons in a menu.
