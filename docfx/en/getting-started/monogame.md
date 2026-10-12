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

4. **Set IcyUI up in your `Game` class.** Call `UseIcyUI()` in `Initialize()`:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#game)]

   `UseIcyUI()` adds IcyUI as a game component and returns the host: `ui.MainCanvas` is the canvas your elements go
   on, and `ui.Configuration` holds every setting. The defaults load the built-in theme and use the system font
   Segoe UI. IcyUI draws during `base.Draw()`, after your own drawing; anything you draw after `base.Draw()` covers
   the UI. A string used as content becomes a <xref:Icy.UI.Controls.TextBlock>.

5. **Run the game.** A "Hello, IcyUI" button appears and reacts to the mouse and keyboard.

> [!NOTE]
> IcyUI doesn't ship a font. On Linux and macOS, or to use your own font, import a font file shipped with the game:
> `this.UseIcyUI(ui => ui.ConfigureFonts().ImportFont("Fonts/MyFont.ttf").WithDefaultFontFamily("My Font"))`. See
> [Fonts on other platforms](stride.md#fonts-on-other-platforms). IcyUI's warnings appear in the debugger's Output
> window.

> [!NOTE]
> On the DesktopGL backend, touch input isn't available, and on Windows on ARM64 the game can hang when it exits.
> WindowsDX doesn't use the SDL layer behind both issues, but touch input and a clean exit on ARM64 haven't been
> verified there yet.

## Next

[Your first screen](first-screen.md) adds a menu described in markup.
