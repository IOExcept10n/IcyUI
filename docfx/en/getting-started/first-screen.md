# Your first screen

This page builds a small main menu: a title and two buttons, described in markup and connected to your game's code
through data binding. It assumes you've finished [Getting started with Stride](stride.md) or
[with MonoGame](monogame.md).

## 1. Describe the menu in markup

Add a file named `Menu.xml` to your game project:

[!code-xml[](../../snippets/markup/first-screen.xml)]

Set its **Copy to Output Directory** property to "Copy if newer", or add this to your project file:

```xml
<ItemGroup>
  <None Update="Menu.xml" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

The markup is plain XML: each element is an IcyUI control, and each attribute sets a property. The three
`{Binding Path=...}` values aren't fixed text: they read `Title`, `PlayCommand` and `QuitCommand` from the menu's
*data context*, an object you supply in code.

## 2. Load it and give it data

# [Stride](#tab/stride)

Replace your startup script with this one (and pick `MenuStartup` in **Add component**):

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#first-screen)]

# [MonoGame](#tab/monogame)

Load the menu at the end of `Initialize()`:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#first-screen)]

---

<xref:Icy.UI.Canvas.LoadMarkup(System.String,System.Object)> loads the markup file from the game's output folder,
sets the view model as the menu's <xref:Icy.UI.UIElement.DataContext> so every `{Binding}` inside reads from it, and
adds the menu to the canvas. Clicking a button, or activating it with the keyboard or a gamepad, runs its command.

## What you built

A menu whose layout lives in markup and whose behaviour lives in C#. Change the markup to restyle or rearrange the menu
without touching code. Because `MenuViewModel` derives from `ObservableObject` (from CommunityToolkit.Mvvm, which IcyUI
already depends on), setting its `Title` while the game runs updates the title on screen. A view model that doesn't
raise `PropertyChanged` is read only once, when the binding is created.

## Next

- The [markup reference](../markup/index.md) describes everything markup can express.
- The [API reference](../api/index.md) documents every type, starting from <xref:Icy.UI.Canvas>.
