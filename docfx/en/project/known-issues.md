# Known issues

## Platforms

- **MonoGame DesktopGL on Windows on ARM64 can hang on exit.** The window closes, but the process stays alive. The cause
  is in the graphics layer that DesktopGL uses on these machines, not in IcyUI. Stride and MonoGame on WindowsDX don't use
  that layer.
- **No touch on MonoGame DesktopGL.** The engine's desktop backend never reports touch input, so touch gestures work only
  where the engine delivers touches (Stride; WindowsDX is not verified). The mouse works everywhere.
- **UI scaling follows the app's DPI awareness.** A Windows app without a PerMonitorV2 `app.manifest` reports a display
  scale of 1 and is stretched by Windows, so the UI looks blurry on a high-DPI display. See [Scaling](../concepts/scaling.md).

## Limits you can run into

- **Themes are chosen at startup.** There is no switching between themes while the game runs.
- **`{Localize}` in a style setter is read once**, when the style loads, so it doesn't follow a language change. Put it on
  the elements, or in a template.
- **Localized text takes no arguments yet.** Format strings such as `"Score: {0}"` are filled in by your code.
- **Keys aren't rebindable.** The focus navigation keys are fixed; see [Input and focus](../concepts/input-and-focus.md).
- **The gamepad's X and Y** raise back and forward events, but a `Frame` doesn't act on them yet.
- **Some controls aren't fully keyboard-operable yet:** list box items, tab headers and a slider's value. Each control's
  page says what it supports.
- **Nested binding paths** follow only the binding source (the data context); see [Data binding](../concepts/data-binding.md).

Found something not listed? Please open an issue on [GitHub](https://github.com/IOExcept10n/IcyUI/issues).
