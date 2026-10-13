# Configuration

An <xref:Icy.Configuration.IcyConfiguration> holds everything IcyUI needs to run in your game: how to draw, where input
comes from, how assets and fonts load, the theme, the log, and how the UI scales. Every element is created with it,
through the canvas it joins. You change it while setting IcyUI up, with a builder.

## Setting it up

`UseIcyUI()` builds a configuration with your engine's defaults:

- rendering, input and asset loading for the engine;
- markup support, so `LoadMarkup` and the asset pipeline read `.xml` UI files;
- the built-in theme;
- system fonts, with Segoe UI as the default family;
- a log provider: the debugger's Output window on MonoGame, Stride's log on Stride.

To change any of it, pass a lambda. It runs after the defaults, so what it sets wins:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#configure)]

After setup, the configuration is the host's <xref:Icy.UI.IIcyHost.Configuration> (or `GetIcyConfiguration()` on the
game).

## Sections

The builder groups settings into sections. Each has a `Configure…` method; most also take a lambda that receives the
section's settings object:

| Section | What it holds | Page |
|---|---|---|
| `ConfigureTheme` | The theme: a resource dictionary of styles and templates. | [Resources, styles and themes](resources-and-styles.md) |
| `ConfigureFonts` | System fonts, imported font files, the default and fallback fonts. | [Assets and fonts](assets-and-fonts.md) |
| `ConfigureScaling` | How the UI scales with the display. | [DPI and UI scaling](scaling.md) |
| `ConfigureLogging` | Where IcyUI's warnings go, and from which level. | [Logging and diagnostics](logging-and-diagnostics.md) |
| `ConfigureGestures` | Hold delay, double-tap delay and drag slop. | [Input, focus and navigation](input-and-focus.md) |
| `ConfigureAssets` | Where files load from, and the importers for each asset type. | [Assets and fonts](assets-and-fonts.md) |
| `ConfigureTypes` | Type conversion (`AddTypeConverter`) and type lookup for markup. | [Markup reference](../markup/index.md) |
| `ConfigureMarkup`, `ConfigureDiagnostics` | Markup options; the debug tools available. | [Logging and diagnostics](logging-and-diagnostics.md) |

A few settings are single values set straight on the builder, like `WithCulture`: the culture strings are localized
in and numbers are shown and parsed in (see [Localization](localization.md) and
[Data binding](data-binding.md#conversion)).

## Order: defaults first, your changes last

Some settings can only be applied once the configuration exists: loading the theme, importing fonts. The builder keeps
those as steps and runs them, in the order they were declared, at the end of the build. Because the engine's defaults
are declared before your lambda, a theme or font family you set replaces the default instead of being overwritten by
it. Setting the same thing twice in your lambda keeps the last one.

So every engine default can be changed or turned off from your lambda:

| Default | To change it or turn it off |
|---|---|
| The built-in theme | `ConfigureTheme(t => t.Theme = myTheme)`; an empty `ResourceDictionary` for no theme at all. |
| System fonts | `ConfigureFonts().UseSystemFonts(false)` (see [Assets and fonts](assets-and-fonts.md#only-your-own-fonts)). |
| Segoe UI as the default family | `ConfigureFonts().WithDefaultFontFamily("My Font")`. |
| The engine's log provider | `ConfigureLogging().UseLoggerFactory(myFactory)` sends everything to your factory instead. |
| Markup support, asset importers | Nothing to undo: they cost nothing until a file of their type is loaded. |

## Using IcyUI without the engine defaults

`UseIcyUI()` is the usual way in. A tool or test that needs a configuration without an engine can build one directly
with <xref:Icy.Configuration.IcyConfigurationBuilder> and the same sections, providing its own rendering and input.

## Pitfalls

- **Call `UseIcyUI()` once per game.** A second call throws; reach the existing host with `GetIcyHost()`.
- **Set up IcyUI at the right moment.** MonoGame: in `Initialize()`, before `base.Initialize()`. Stride: in a startup
  script's `Start()`, or after `BeginRun()` in a code-only game, once the scene's graphics compositor exists.

## See also

- [Canvas, the host and the element tree](canvas-and-host.md).
- [Fonts on other platforms](../getting-started/stride.md#fonts-on-other-platforms).
