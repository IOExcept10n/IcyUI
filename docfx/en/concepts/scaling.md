# DPI and UI scaling

A UI laid out in pixels is tiny on a 4K laptop screen and huge on a small one. IcyUI lays out in UI units and the
canvas multiplies them by a scale factor when it draws, so the UI keeps a sensible physical size. The factor comes from
the display, from a resolution you design for, or both, times a setting the player can change.

## Scale modes

<xref:Icy.Configuration.ScalingConfiguration.Mode> chooses where the base factor comes from:

| Mode | Base factor | Suits |
|---|---|---|
| `Dpi` (default) | The display's scale: 1 at 100 %, 1.5 at 150 % in Windows' display settings. | Tools, editors, UI that should match the OS. |
| `ReferenceResolution` | The window size divided by <xref:Icy.Configuration.ScalingConfiguration.ReferenceSize>. | Games: design once for 1920 × 1080, scale to any window. |
| `None` | 1: one unit is one pixel. | Pixel-exact UI, screenshots. |

With `ReferenceResolution`, <xref:Icy.Configuration.ScalingConfiguration.ReferenceFit> decides what happens when the
window's aspect ratio differs: `Fit` uses the smaller ratio, so the whole reference area is always visible; `Fill` uses
the larger one, so it fills the window and may extend past it.

<xref:Icy.Configuration.ScalingConfiguration.UserScale> multiplies the result: it's the "UI size" setting players
expect in a game's options.

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#scaling)]

A single canvas can override the mode with `Canvas.ScaleMode`, for example a debug overlay at `None`. The factor in use is
<xref:Icy.UI.Canvas.EffectiveScale>.

## Display scale and the app manifest

`Dpi` mode reads the display scale through Windows. A game must declare itself DPI-aware for that: an `app.manifest` with
`PerMonitorV2` DPI awareness. Without it, Windows reports a scale of 1 and stretches the whole window as a bitmap, so text
looks blurry at 150 %. Both IcyUI sample games ship such a manifest; copy theirs.

## A UI-size setting

Changing `UserScale` re-lays out the UI at once. For a setting in your options menu, follow the usual pattern for
display settings: apply the new size, ask the player to confirm, and revert automatically after a few seconds if they
don't, so an unreadable size can't lock them out.

## Pitfalls

- **Pixel sizes in your own drawing.** If your game draws next to the UI, multiply by
  <xref:Icy.UI.Canvas.EffectiveScale> to line up with it.
- **Changing monitors.** With `Dpi`, moving the window to a monitor with a different scale changes the factor; the UI
  re-lays out on its own.

## See also

- [Layout](layout.md).
- [Configuration](configuration.md).
