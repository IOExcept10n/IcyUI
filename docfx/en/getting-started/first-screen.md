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

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#first-screen)]

Call `MainMenu.Show(canvas, configuration)` from your startup script, in place of the "Hello, IcyUI" button.

# [MonoGame](#tab/monogame)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#first-screen)]

Call `MainMenu.Show(canvas, configuration, viewModel)` at the end of `Initialize()`. For the view model, use a class
with a `Title` string and `PlayCommand`/`QuitCommand` commands, like the `MenuViewModel` in the Stride tab.

---

<xref:Icy.Markup.MarkupLoader> turns the markup into elements. Setting <xref:Icy.UI.UIElement.DataContext> on the
root makes every `{Binding}` inside it read from your view model. Clicking a button, or activating it with the keyboard
or a gamepad, runs its command.

## What you built

A menu whose layout lives in markup and whose behaviour lives in C#. Change the markup to restyle or rearrange the menu
without touching code, or change the view model's `Title` to see binding at work.

## Next

- The [markup reference](../markup/index.md) describes everything markup can express.
- The [API reference](../api/index.md) documents every type, starting from <xref:Icy.UI.Canvas>.
