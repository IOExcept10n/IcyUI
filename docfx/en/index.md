---
_layout: landing
---

# IcyUI

IcyUI is a UI library for .NET games. You describe screens in XML markup or in C#, style them with themes and
control templates, bind them to your game's data, and drive them with mouse, keyboard, gamepad or touch. The core is
engine-agnostic; thin integration packages connect it to **Stride** and **MonoGame**.

> [!NOTE]
> IcyUI is in early alpha (`0.1.0-alpha`). It's usable, but APIs may change between alpha releases.

## Install

Pick the package for your engine. It brings in the core `IcyUI` package with it.

```
dotnet add package IcyUI.Stride --prerelease
dotnet add package IcyUI.MonoGame --prerelease
```

## Where to go next

- **[Getting started](getting-started/installation.md)**: install IcyUI and put your first screen in a Stride or
  MonoGame game.
- **[Markup reference](markup/index.md)**: the XML dialect for describing UI.
- **[API reference](api/index.md)**: every public type and member, generated from the source.

## What's inside

- XML markup with data binding, resources, styles, visual states and control templates.
- A built-in default theme.
- Layout panels: `Grid`, `StackPanel`, `WrapGrid`, `SplitPane`, and a virtualizing `ScrollViewer`.
- Controls such as `Button`, `CheckBox`, `Slider`, `TextBox`, `ComboBox`, `ListBox`, `TreeView`, `TabControl`,
  `Expander`, `Dialog`, `ColorPicker` and `PropertyGrid`.
- Keyboard and gamepad navigation between controls, touch gestures, drag and drop, and DPI-aware scaling.
- Animations.
- An in-game UI editor with hot reload, in the separate `IcyUI.Design` package.

IcyUI is open source under the MIT license: [github.com/IOExcept10n/IcyUI](https://github.com/IOExcept10n/IcyUI).
