# Getting started with Stride

This page puts an IcyUI button on screen in a Stride game. Most Stride games are Game Studio projects, so that path
comes first; if you write your game in code only, skip to [Without Game Studio](#without-game-studio).

## In a Game Studio project

1. **Create a project** in the Stride launcher (Stride 4.4 or later; any template works, for example "New Game").

2. **Add the package** to the game project, `MyGame/MyGame.csproj` (the shared one, not the platform project):

   ```
   dotnet add MyGame/MyGame.csproj package IcyUI.Stride --prerelease
   ```

3. **Add a startup script.** Create `IcyUIStartup.cs` in the game project:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#startup-script)]

   `UseIcyUI()` adds IcyUI to the game and draws it on top of whatever your scene renders. It returns the host:
   `ui.MainCanvas` is the canvas your elements go on, and `ui.Configuration` holds every setting. The defaults load
   the built-in theme and use the system font Segoe UI. A string used as content becomes a <xref:Icy.UI.Controls.TextBlock>.

4. **Attach the script.** Reopen the project in Game Studio and let it reload the scripts. In your startup scene,
   select an entity (the camera is fine), then in the property grid choose **Add component** and pick `IcyUIStartup`.

5. **Run the game.** A "Hello, IcyUI" button appears over the scene and reacts to the mouse and keyboard.

Other scripts reach the same UI with `Game.GetIcyCanvas()` or `Game.GetIcyHost()`.

## Without Game Studio

If you derive your own `Game` class and set up the scene in code, set up IcyUI in `BeginRun()`, after the graphics
compositor exists:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#code-only)]

`UseIcyUI()` draws through `SceneSystem.GraphicsCompositor`, so it must not be `null` at that point.

## Fonts on other platforms

IcyUI doesn't ship a font: by default it uses Segoe UI from the Windows fonts folder. On Linux and macOS, or to use
your own font everywhere, ship a font file with the game (set its **Copy to Output Directory** to "Copy if newer") and
import it:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Fonts.cs#own-font)]

Don't copy Windows fonts into your game: their license doesn't allow redistributing them. Free fonts under the SIL Open
Font License, such as Noto Sans or Inter, can be shipped with any game. When a font can't be found, IcyUI writes a
warning to Stride's log.

## Troubleshooting

- **The project was created with Stride 4.3 or earlier.** IcyUI targets .NET 10. Upgrade the project to Stride 4.4
  from the launcher, or set `<TargetFramework>net10.0</TargetFramework>` in every project file of the solution (the
  Windows platform project may use `net10.0-windows`; keep its suffix).
- **Game Studio doesn't start on Windows on ARM64** ("no compatible MSBuild found"). Use the code-only setup there,
  or edit the project in Game Studio on an x64 machine.
- **Text doesn't appear.** Check the game's log for an IcyUI warning about a missing font; see
  [Fonts on other platforms](#fonts-on-other-platforms).

## Next

[Your first screen](first-screen.md) replaces the button with a menu described in markup.
