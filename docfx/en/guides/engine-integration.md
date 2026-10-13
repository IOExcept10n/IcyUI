# Writing an engine integration

IcyUI draws and reads input through a few interfaces, and an engine integration implements them. This guide lists what
one provides, in the order you build it, using FNA as the target. FNA has the XNA API, so the skeleton looks like the
MonoGame integration, which is the complete reference:
[`IcyUI.MonoGame` on GitHub](https://github.com/IOExcept10n/IcyUI/tree/main/sources/IcyUI.MonoGame).

> [!NOTE]
> `IcyUI.FNA` is a stub today; there is no FNA package yet. This guide describes how an integration is built, not a
> shipped one. FNA-specific details, such as its native libraries, are out of scope.

You need a good grasp of the engine you target and of [Canvas, the host and the element tree](../concepts/canvas-and-host.md).
A game never needs any of this: it only uses an integration that exists.

## What an integration provides

| Piece | Interface | Does |
|---|---|---|
| Drawing | `IRenderContext` | Draws textured quads inside a sprite batch, makes textures, reports the viewport and display scale. |
| Input | `IInputSystem` | Turns keyboard, mouse, gamepad and touch state into IcyUI's events; text input and the clipboard. |
| Assets | `IAssetContextFactory`, `IPlatformAssetImporter<T>` | Opens the engine's content and turns image streams into textures. |
| Host | `IIcyHost` | A game component that connects the three to the game loop and owns the canvases. |
| Entry point | `UseIcyUI(this Game, ...)` | Builds the configuration and adds the host. |

IcyUI's core never references the engine; everything above is the only code that does.

## 1. The render context

`IRenderContext` draws only textured quads: the canvas calls `Begin`, then `Draw` for each rectangle, glyph and image,
then `End`. A solid color is a tinted draw of `WhiteTexture`.

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/Guides/EngineIntegration.cs#render-context-outline)]

Points that went wrong before, so look at them in the MonoGame version:

- **Straight alpha.** IcyUI's textures aren't premultiplied, so the sprite batch must blend with straight alpha; the
  default premultiplied blend makes anti-aliased text edges too bright.
- **Scissor test.** Clipping uses the scissor rectangle, and the rasterizer state must enable the scissor test or every
  clip is silently ignored.
- **`Flush`** is a no-op outside `Begin`/`End`; it ends and restarts the batch when the scissor or an effect changes.
- **Display scale.** Report the monitor's DPI scale (`DisplayScale`, `DisplayScaleChanged`), polled each update, so
  [scaling](../concepts/scaling.md) works. Core supplies the platform providers.

## 2. Input

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/Guides/EngineIntegration.cs#input-outline)]

Each device implements its interface by reading the engine's state in `Update` and raising IcyUI's events (pressed,
released, moved, navigation). Text input comes from the engine's window (character events, IME) rather than from key
presses. Leave a device `null` if the engine can't report it; the canvas copes. The clipboard is the one piece with an
OS dependency: pick it with the core's clipboard selector instead of writing one per engine.

## 3. Assets

An asset context opens content by URI (the engine's content pipeline, files, embedded resources), and an importer turns
an image stream into the engine's texture. Register both on the asset configuration; `IcyUI.MonoGame` has a
`MonoGameAssetContextFactory` and a `TextureImporter` of a few lines each.

## 4. The host

The host connects the pieces to the game loop. It draws last, so the UI is on top of the game:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/Guides/EngineIntegration.cs#host)]

## 5. The entry point

The extension builds the configuration with the engine's defaults, then runs the game's lambda so it can replace any of
them, then adds the host:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/Guides/EngineIntegration.cs#use-icyui)]

Throw if the game calls it twice, and offer `GetIcyHost` and `GetIcyCanvas` so code can reach the host later; the
MonoGame version shows both.

## What you built

The five pieces an engine needs, in the order that lets you test each: draw a rectangle, read a click, load a texture,
then run a markup page through the host.

## Next

- [Canvas, the host and the element tree](../concepts/canvas-and-host.md): what the host does with the canvases.
- [Configuration](../concepts/configuration.md): the builder the entry point fills.
