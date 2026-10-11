# IcyUI

A cross-engine UI library for .NET games. The engine-agnostic core handles layout, controls, markup, styling, templating, data binding, input and animations. Thin integration packages connect it to a specific engine's rendering and input.

> **Status:** early alpha (`0.1.0-alpha`). Published on NuGet as prerelease packages; APIs may change between alphas.

**Documentation:** [icyui.dev](https://icyui.dev) · [на русском](https://icyui.dev/ru/)

## Engines

| Package | Status |
|---|---|
| `IcyUI.Stride` | Integration for Stride 4.3+ |
| `IcyUI.MonoGame` | Integration for MonoGame 3.8.5.1+, backend-neutral (DesktopGL or WindowsDX) |
| `IcyUI.FNA` | Planned |

```
dotnet add package IcyUI.Stride --prerelease
dotnet add package IcyUI.MonoGame --prerelease
```

See [Getting started](https://icyui.dev/getting-started/installation.html) for the requirements (.NET 10) and the setup for each engine.

## Features

- XML markup with bindings, `{StaticResource}`, styles, visual states and control templates.
- A built-in default theme.
- Layout panels: `Grid`, `StackPanel`, `WrapGrid`, `SplitPane`, `ScrollViewer` (with virtualization).
- Controls: `Button`, `ToggleButton`, `CheckBox`, `Slider`, `ProgressBar`, `TextBox`, `ComboBox`, `ListBox`, `TreeView`, `TabControl`, `Expander`, `Window`, `Dialog`/`MessageBox`, `ColorPicker`, `PropertyGrid`, and more.
- Keyboard and gamepad navigation between controls, touch gestures, drag-and-drop, and an overlay layer for popups and dialogs.
- DPI-aware UI scaling.
- Animations and timelines.
- An in-game UI editor with markup hot reload (`IcyUI.Design`).

## Building

Requires the .NET 10 SDK.

```
dotnet build sources/IcyUI.sln
dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj
```

Sample hosts for both engines live in `sources/MonoGame Sample` and `sources/Stride Sample`. Their demos are shared from `sources/Shared Samples`. The documentation site is built from `docfx/` (see `CLAUDE.md`).

## History

The project started as MysticUI, a Stride UI library based on [Myra](https://github.com/rds1983/Myra), and was later renamed AquaUI. It has since been rewritten as IcyUI.

## License

[MIT](LICENSE)
