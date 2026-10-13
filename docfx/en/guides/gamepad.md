# Gamepad-friendly UI

This guide makes a screen fully operable with a gamepad (or the arrow keys): focus that starts in the right place,
layouts the D-pad can walk, dialogs that keep focus, and a switch for gameplay. The screen is a loadout page: a grid of
buttons, a row of toggles, a check box, a combo box and a Start button.

You need a page loaded from markup ([Your first screen](../getting-started/first-screen.md)). How focus works is
described in [Input, focus and navigation](../concepts/input-and-focus.md).

## 1. A layout the D-pad can walk

Arrow keys and the D-pad move focus to the nearest focusable control in that direction, so no tab order is needed. Plain
panels and grids work as they are:

[!code-xml[](../../snippets/markup/guides/gamepad/gamepad-menu.xml)]

Two things keep it reliable: leave a gap between controls instead of overlapping them, and keep related controls in
rows and columns so "the nearest one to the right" is the one the player expects.

## 2. Focus the first control on open

Nothing focused means nothing for the D-pad to move. Focus the first button when the page is shown:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/Gamepad.cs#focus-on-open)]

## 3. Make the focus visible

A gamepad player has no pointer to follow, so the focused control must stand out. The built-in theme gives every
control a `FocusStates` group with a blue border; a game theme can make it bolder with an implicit style, as described in
[Custom themes](custom-themes.md). Test it from the couch: if you can't see the focus from a metre away, strengthen it.

## 4. Dialogs

An open `Window` or `Dialog` is a focus scope: the D-pad stays inside it, B closes it, and focus goes back to the control
that had it:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/Gamepad.cs#confirm-dialog)]

For your own panel with the same behaviour, set `IsFocusScope` on it.

## 5. Lists and scrolling

A list or a scroll viewer scrolls a focused item into view, and a combo box opens with A and takes the D-pad up and
down while open; B closes it.

> [!NOTE]
> The items of a list box can't be selected with the keyboard or a gamepad yet; see [Known issues](../project/known-issues.md).

## 6. Gameplay

While the player controls the game, the D-pad and the stick shouldn't also move the menu focus. Switch navigation off
for gameplay and on again for menus:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/Gamepad.cs#gameplay-switch)]

The focused control still receives arrows while navigation is off (a text box moves its caret), only moving focus to
another element is disabled.

## The keys

| Keyboard | Gamepad | Does |
|---|---|---|
| Arrow keys | D-pad | Move focus to the nearest focusable element in that direction. |
| Tab, Shift+Tab | Right / left stick click | Move focus to the next / previous element. |
| Enter | A | Activate the focused element. |
| Escape | B | Close the open dialog or list. |

> [!NOTE]
> These keys aren't rebindable yet; configurable bindings are on the roadmap.

## What you built

A page where every control can be reached and used with the D-pad, that starts with a focus, keeps dialogs self-contained
and hands the buttons back to the game during play.

## Next

- [Building a game menu](game-menu.md): a menu to apply this to.
- [Dialogs and windows](../controls/dialogs.md): focus scopes in detail.
