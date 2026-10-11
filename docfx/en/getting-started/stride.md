# Getting started with Stride

This page puts an IcyUI button on screen in a Stride game. Most Stride games are Game Studio projects, so that path
comes first; if you write your game in code only, skip to [Without Game Studio](#without-game-studio).

## In a Game Studio project

1. **Create a project** in the Stride launcher (any template works, for example "New Game").

2. **Retarget it to .NET 10.** Close Game Studio, then in every project file of the solution
   (`MyGame/MyGame.csproj` and the platform project such as `MyGame.Windows/MyGame.Windows.csproj`) set:

   ```xml
   <TargetFramework>net10.0</TargetFramework>
   ```

   The Windows platform project may use `net10.0-windows` instead; keep whatever suffix it had.

3. **Add the package** to the game project, `MyGame/MyGame.csproj` (the shared one, not the platform project):

   ```
   dotnet add MyGame/MyGame.csproj package IcyUI.Stride --prerelease
   ```

4. **Add a startup script.** Create `IcyUIStartup.cs` in the game project:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#startup-script)]

   `UseIcyUI()` adds IcyUI to the game and wraps your scene's graphics compositor, so the UI draws on top of whatever
   your scene renders. `GetIcyConfiguration()` returns the configuration every IcyUI element is created with, and
   `UseDefaultTheme()` loads the built-in theme.

5. **Attach the script.** Reopen the project in Game Studio and let it reload the scripts. In your startup scene,
   select an entity (the camera is fine), then in the property grid choose **Add component** and pick `IcyUIStartup`.

6. **Run the game.** A "Hello, IcyUI" button appears over the scene and reacts to the mouse and keyboard.

## Without Game Studio

If you derive your own `Game` class and set up the scene in code, configure IcyUI in `BeginRun()`, after the graphics
compositor exists:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#code-only)]

`UseIcyUI()` wraps `SceneSystem.GraphicsCompositor`, so it must not be `null` at that point.

> [!NOTE]
> IcyUI draws on top of your scene's compositor output. Elements are only visible once they are added to a
> <xref:Icy.UI.Canvas> that's in the overlay's `Canvases` collection.

## Next

[Your first screen](first-screen.md) replaces the button with a menu described in markup.
