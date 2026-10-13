# IcyUI.Stride

Stride integration for [IcyUI](https://www.nuget.org/packages/IcyUI). Requires Stride 4.4 or later.

```
dotnet add package IcyUI.Stride --prerelease
```

```csharp
using Icy.Stride.Configuration;
using Icy.UI.Controls;

// In a startup script's Start() (or Game.BeginRun() after the scene system is set up):
StrideIcyHost ui = Game.UseIcyUI();
ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });
```

**Status:** early alpha (0.1.0-alpha). APIs may change between alphas.

Documentation: https://icyui.dev/getting-started/stride.html
Source and samples: https://github.com/IOExcept10n/IcyUI
Issues: https://github.com/IOExcept10n/IcyUI/issues
