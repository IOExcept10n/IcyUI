# Input, focus and navigation

IcyUI takes input from the mouse, keyboard, gamepad and touch through the engine integration, and routes it to the
element it's meant for: the element under the pointer, or the element with focus. A game played with a gamepad can
use every control without a mouse.

## Pointer input

The mouse and touch go to the topmost element under the pointer: overlays first (popups, dialogs), then root elements
from the top of the drawing order down. An element with <xref:Icy.UI.UIElement.IsHitTestVisible> off, hidden, or fully
transparent is skipped, and so is everything outside a `ClipToBounds` parent.

### What gets hit

An element is hit where it paints. A panel (`Grid`, `StackPanel`) never is, only its children are; a `Border`, a
`Page` or a `Frame` is hit only where it has a `Background`. So a full-screen layout with buttons in its corners passes
every click on its empty part through to the game, and `IsMouseOverGUI` is `false` there. A transparent background
paints nothing but is hit: use `Background="Transparent"` to make an invisible area clickable.

Text, images and icons are hit over their whole area, and so are the controls that take the pointer themselves
(buttons, check boxes, list and tree rows, sliders, text boxes, combo boxes), with or without a background. Dialogs
block everything behind them, scroll viewers scroll from anywhere in them, and a split pane can always be grabbed by
its divider.

### Taps

A tap goes to the element under it, then to each of its ancestors, until one handles it. Subscribe to `Tapped` on any
element: `e.GetPosition(element)` gives the point in that element's space, `e.Source` the element that was hit, and
`e.Handled = true` keeps the tap from the ancestors. Buttons, list items and tree view rows handle the taps they act on,
so a button inside a tappable panel doesn't trigger both.

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#tapped)]

Touch adds gestures, recognized from the raw touches and the mouse alike: a tap, a hold, a drag, and a two-finger pinch.
`ConfigureGestures` tunes them: `HoldDelay` (0.5 s), `MultiTapDelay` (0.3 s) and `Slop` (10 px of movement before a
press becomes a drag).

> [!NOTE]
> MonoGame's DesktopGL backend reports no touch input; the mouse still works.

## Focus

One element at a time has keyboard and gamepad focus: the canvas's <xref:Icy.UI.Canvas.FocusedElement>. Controls the
player can operate are focusable (<xref:Icy.UI.UIElement.IsFocusable>); a click or tap focuses them, and
`Canvas.Focus(element)` does it from code, for example to start a menu on its first button.

| Keyboard | Gamepad | Does |
|---|---|---|
| Arrow keys | D-pad | Move focus to the nearest focusable element in that direction. |
| Tab, Shift+Tab | Right / left stick click | Move focus to the next / previous element. |
| Enter | A | Activate the focused element (press a button, toggle a check box, open a list). |
| Escape | B | Close the open dialog or list. |

Arrow-key navigation is spatial: it picks the nearest element in the pressed direction on screen, so it works for any
layout without setting an order. A focused element is scrolled into view. Some controls use the arrows themselves while
focused (a text box moves its caret, a tree view moves between rows) and pass them on at their edges.

> [!NOTE]
> The gamepad's X and Y raise back and forward events (`INavigationEvents.NavigateBack` and `NavigateForward`) for your own
> code to handle, but a `Frame` doesn't act on them yet. Give a page a Back button that calls
> `Frame.Navigation.TryNavigateBack()`.

A **focus scope** (<xref:Icy.UI.UIElement.IsFocusScope>) keeps navigation inside it: an open `Window` or `Dialog` is
one, and focus returns to where it was when it closes. Set `Canvas.IsKeyboardNavigationEnabled` to `false` to turn
arrow and Tab navigation off, for example while gameplay uses those keys.

## Drag and drop

Moving data between elements (an item from one inventory slot to another) uses two interfaces. A source implements
`IDragSource` and decides, when a drag starts on it, what it carries and what follows the pointer; a target implements
`IDropTarget` and decides whether it accepts the drop:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#drag-drop)]

The canvas finds the source by walking up from the element under the pointer, moves the preview with the pointer, and
asks the target under it at each step. Dragging that only moves something inside one control (a slider's thumb, a split
pane's divider) is built into those controls.

## Pitfalls

- **UI input and game input.** Check <xref:Icy.UI.Canvas.IsMouseOverGUI> before your game reacts to a click. Give
  full-screen layout no background, so its empty areas don't count as UI; see
  [Canvas, the host and the element tree](canvas-and-host.md#ui-input-and-game-input).
- **Nothing focused, nothing to navigate.** A menu opened for a gamepad should focus its first control.
- **Some controls aren't keyboard-operable yet:** list box items, tab headers and a slider's value; see each control's
  page.

## See also

- [Buttons](../controls/buttons.md) and the other controls pages, for each control's keys.
- [Dialogs and windows](../controls/dialogs.md) for focus scopes.
