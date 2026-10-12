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

<div class="row row-cols-1 row-cols-md-3 g-3 mb-4">
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="getting-started/installation.md"><strong>Getting started</strong><span>Install IcyUI and put a first screen in a Stride or MonoGame game.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="concepts/index.md"><strong>Concepts</strong><span>Layout, binding, styles, input and the rest of the model.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="controls/index.md"><strong>Controls</strong><span>Every control, with screenshots and markup.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="guides/index.md"><strong>Guides</strong><span>Menus, custom controls, themes, the editor.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="markup/index.md"><strong>Markup reference</strong><span>The XML dialect for describing UI.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="api/index.md"><strong>API reference</strong><span>Every public type and member.</span></a></div>
</div>

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
