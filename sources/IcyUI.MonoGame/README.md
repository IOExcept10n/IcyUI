# IcyUI.MonoGame

MonoGame integration for [IcyUI](https://www.nuget.org/packages/IcyUI).

```
dotnet add package IcyUI.MonoGame --prerelease
```

**Pick a MonoGame backend yourself.** This package doesn't depend on one. Reference `MonoGame.Framework.DesktopGL`
or `MonoGame.Framework.WindowsDX`, version **3.8.5.1 or later**. Older MonoGame versions fail at load time with a `FileLoadException`.

```csharp
using Icy.MonoGame.Configuration;
using Icy.UI.Controls;

// In Game.Initialize(), before base.Initialize():
MonoGameIcyHost ui = this.UseIcyUI();
ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });
// IcyUI draws itself during base.Draw().
```

**Known issues (DesktopGL):**
- On Windows-on-ARM64, `Game.Dispose()` can hang on shutdown (OpenGL runs through `OpenGLOn12`).
- No touch input: DesktopGL doesn't fill `TouchPanel`.

**Status:** early alpha (0.1.0-alpha). APIs may change between alphas.

Documentation: https://icyui.dev/getting-started/monogame.html
Source and samples: https://github.com/IOExcept10n/IcyUI/tree/platform-independent
Issues: https://github.com/IOExcept10n/IcyUI/issues
