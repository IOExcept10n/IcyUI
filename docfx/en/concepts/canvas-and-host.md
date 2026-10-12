# Canvas, the host and the element tree

Everything IcyUI draws lives on a <xref:Icy.UI.Canvas>: a surface the size of your game's window that lays out its
elements, draws them every frame and routes mouse, keyboard, gamepad and touch input to them. `UseIcyUI()` creates the
host that connects canvases to your engine and gives you the first canvas.

## The host

`UseIcyUI()` returns the engine's host: `MonoGameIcyHost` or `StrideIcyHost`. Both are an <xref:Icy.UI.IIcyHost>:

- <xref:Icy.UI.IIcyHost.Configuration>: the settings every element is created with (see [Configuration](configuration.md));
- <xref:Icy.UI.IIcyHost.MainCanvas>: the canvas `UseIcyUI()` created, with input enabled;
- <xref:Icy.UI.IIcyHost.Canvases>: every canvas the host draws each frame, in order.

The host pumps input and follows the display scale every frame. On MonoGame it's a game component that draws during
`base.Draw()`, after your own drawing; on Stride it draws through the graphics compositor, after the scene. Other code
reaches the same host with `GetIcyHost()` (or `GetIcyCanvas()` for the main canvas) on the game.

A later canvas in <xref:Icy.UI.IIcyHost.Canvases> draws on top of an earlier one. A second canvas is useful for a
HUD that stays above menus, or a debug layer:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#canvases)]

## The element tree

A canvas holds root elements (<xref:Icy.UI.Canvas.Add(Icy.UI.UIElement)>, or `LoadMarkup` for a markup file). Panels and
controls hold children, so the whole UI is a tree. Each element knows its <xref:Icy.UI.UIElement.Parent>; a root's
parent is `null`, and it belongs to the canvas.

- **Drawing order:** children draw after their parent, in order; <xref:Icy.UI.UIElement.ZIndex> moves an element above
  or below its siblings.
- **Overlays:** popups, dropdown lists, tooltips and dialogs go to the canvas's <xref:Icy.UI.Canvas.Overlays>, which
  draw above every root element and receive input first.
- **Visibility:** <xref:Icy.UI.UIElement.IsVisible> hides an element and its children and stops their input, but
  they keep their space in the layout; <xref:Icy.UI.UIElement.Opacity> fades them (at 0 they aren't drawn or hit);
  <xref:Icy.UI.UIElement.IsHitTestVisible> set to `false` lets clicks pass through.

## UI input and game input

The canvas takes the input that's meant for the UI. Your game should ignore a click that landed on a button: check
<xref:Icy.UI.Canvas.IsMouseOverGUI> before acting on the mouse.

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#game-input)]

Which element has keyboard and gamepad focus is <xref:Icy.UI.Canvas.FocusedElement>; see
[Input, focus and navigation](input-and-focus.md).

## Pitfalls

- **An element belongs to one parent.** Remove it from its panel (or canvas) before adding it somewhere else.
- **Elements created in code have no theme until they're attached.** The theme's styles apply when an element joins a
  canvas, so read `Template` or theme colors after `Add`, not before.
- **Hidden still takes space.** To take an element out of the layout, remove it from its panel; set
  <xref:Icy.UI.UIElement.IsVisible> back when it should only reappear in place.
- **Draw your own content before the UI.** On MonoGame, anything you draw after `base.Draw()` covers the UI.

## See also

- [Configuration](configuration.md): what `UseIcyUI()` sets up.
- [Layout](layout.md): how elements on a canvas get their size and position.
- [Getting started with Stride](../getting-started/stride.md) and [with MonoGame](../getting-started/monogame.md).
