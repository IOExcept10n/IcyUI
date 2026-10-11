# Getting started with MonoGame

This page sets up IcyUI in a MonoGame game.

1. **Create a project** from a MonoGame template:

   ```
   dotnet new mgdesktopgl -o MyGame
   ```

   or `dotnet new mgwindowsdx` for the Direct3D backend. Both work with IcyUI.

2. **Retarget it to .NET 10.** In `MyGame/MyGame.csproj`, set `<TargetFramework>net10.0</TargetFramework>`
   (or `net10.0-windows` if the template used a `-windows` suffix), and make sure the MonoGame packages are 3.8.5.1 or
   later.

3. **Add the package:**

   ```
   dotnet add MyGame/MyGame.csproj package IcyUI.MonoGame --prerelease
   ```

4. **Set IcyUI up in your `Game` class.** Call `UseIcyUI()` in `Initialize()` and render the canvas in `Draw()`:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#game)]

   `UseIcyUI()` registers IcyUI as a game component, `GetIcyConfiguration()` returns the configuration every IcyUI
   element is created with, and `UseDefaultTheme()` loads the built-in theme. A <xref:Icy.UI.Canvas> is the root of
   everything IcyUI draws: add elements to it with <xref:Icy.UI.Canvas.Add(Icy.UI.UIElement)>.

5. **Run the game.** The canvas is empty for now; [Your first screen](first-screen.md) fills it.

> [!NOTE]
> On the DesktopGL backend, touch input isn't available, and on Windows on ARM64 the game can hang when it exits.
> WindowsDX doesn't use the SDL layer behind both issues, but touch input and a clean exit on ARM64 haven't been
> verified there yet.

## Next

[Your first screen](first-screen.md) adds a menu described in markup.
