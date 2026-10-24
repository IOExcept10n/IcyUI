# Dialogs and windows

![A window asking "Quit to the main menu?" with Quit and Cancel buttons](../../images/controls/dialogs.png)

A <xref:Icy.UI.Controls.Window> is a framed panel that keeps keyboard and gamepad focus inside itself while it's open,
for an in-game menu or an inventory. A <xref:Icy.UI.Controls.Dialog> is modal: it covers the screen with a dimmed
backdrop, blocks the mouse and touch behind it, and closes with Escape or the gamepad's B button.
<xref:Icy.UI.Controls.MessageBox> shows a ready-made dialog with a message and buttons, and returns the player's
answer.

## Markup

[!code-xml[](../../snippets/markup/controls/dialogs.xml)]

A window is added to the canvas like any element and drawn only while open: call `Show()` to open it and `Close()` to
close it.

For a question with a standard answer, use a message box from code:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Controls.cs#message-box)]

## Key properties

| Property | Description |
|---|---|
| <xref:Icy.UI.Controls.Window.IsOpen> | Whether the window is open. |
| <xref:Icy.UI.Controls.ContentControl.Content> | What the window or dialog shows. |

## Events and commands

<xref:Icy.UI.Controls.Window.Opened> and <xref:Icy.UI.Controls.Window.Closed> for a window;
<xref:Icy.UI.Controls.Dialog.Closed> for a dialog. <xref:Icy.UI.Controls.Dialog%601> adds a result: its `ShowAsync`
returns a task that completes with the value the dialog closed with.

## Styling

The default theme gives windows a dark background and a border. A dialog's template draws the backdrop and centers its
content box.

## Keyboard, gamepad and touch

While a window or dialog is open, Tab, the arrow keys and the D-pad move focus only among its controls, and focus
returns to where it was when it closes. Escape or B closes a dialog or message box (a message box then returns
`DialogResult.None`).

> [!NOTE]
> A window doesn't block the mouse or touch: controls behind it stay clickable. Use a dialog when nothing else may be
> used until the player answers.

## Window

Not modal for the pointer; modal for focus.

## Dialog

Shown with `Show(canvas)` as an overlay above everything on the canvas.

## MessageBox

`ShowAsync(canvas, message, buttons)` with `DialogButtons.OK`, `OKCancel`, `YesNo` or `YesNoCancel`; and
`ShowInputAsync(canvas, prompt)` for a line of text.

The button captions are localized: the keys are `OK`, `Cancel`, `Yes` and `No`, in the context `Icy.MessageBox`
(`msgctxt` in a `.po` file). Without a translation they show in English. For captions of your own, such as "Quit"
instead of "OK", pass them to `ShowAsync(canvas, message, buttons, result => ...)`. See
[Localization](../concepts/localization.md).

## See also

- [Buttons](buttons.md) for a dialog's actions.
