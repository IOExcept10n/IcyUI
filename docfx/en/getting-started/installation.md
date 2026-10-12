# Installation

## Requirements

- **.NET 10 SDK.** IcyUI targets `net10.0`, so your game project must target `net10.0` (or `net10.0-windows`) too.
- **Windows**, x64 or ARM64.
- **An engine:** Stride 4.4 or later, or MonoGame 3.8.5.1 or later.

> [!IMPORTANT]
> New projects from the MonoGame templates target an older .NET version. Change `<TargetFramework>` to `net10.0` in
> your game's project file before adding IcyUI, or NuGet refuses the package (error `NU1202`); the
> [MonoGame page](monogame.md) shows where. Stride 4.4 projects already target .NET 10.

## Packages

| Package | What it is | When you need it |
|---|---|---|
| `IcyUI.Stride` | Stride integration: rendering, input, assets. | Your game uses Stride. |
| `IcyUI.MonoGame` | MonoGame integration: rendering, input, assets. | Your game uses MonoGame. |
| `IcyUI` | The engine-agnostic core: controls, layout, markup, binding. | Installed automatically with an engine package. |
| `IcyUI.Design` | Markup design document, hot reload and the in-game editor. | Only for development tools; a shipped game doesn't need it. |

All packages are prereleases while IcyUI is in alpha, so add `--prerelease`:

```
dotnet add package IcyUI.Stride --prerelease
```

## MonoGame: choose a backend yourself

`IcyUI.MonoGame` doesn't depend on a particular MonoGame backend. Your game references one of them, as it already does
when created from a MonoGame template:

- `MonoGame.Framework.DesktopGL` (OpenGL),
- `MonoGame.Framework.WindowsDX` (Direct3D 11).

Use version 3.8.5.1 or later. With an older MonoGame, the game starts but fails with a `FileLoadException` the first time
it uses IcyUI.

## Next

- [Getting started with Stride](stride.md)
- [Getting started with MonoGame](monogame.md)
