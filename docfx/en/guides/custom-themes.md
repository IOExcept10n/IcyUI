# Custom themes

This guide gives a game its own look: a palette that recolors the built-in buttons and lists, a new template for one
control, and, when that isn't enough, a whole theme of your own. A theme is a file, so an artist can change it without
touching code.

You need a game with IcyUI set up ([Getting started](../getting-started/stride.md)) and the idea of styles and templates
from [Resources, styles and themes](../concepts/resources-and-styles.md) and
[Control templates and visual states](../concepts/templates-and-visual-states.md).

## 1. Layer a theme file over the built-in theme

`ExtendDefaultTheme` loads a resource-dictionary file and layers it over the built-in theme. Everything the file doesn't
mention keeps its built-in look:

# [Stride](#tab/stride)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/CustomThemesStride.cs#theme-setup)]

# [MonoGame](#tab/monogame)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/Guides/CustomThemesMonoGame.cs#theme-setup)]

---

The path is relative to the game's output folder, like any markup file: add `Themes/MyGame.xml` to the project with
**Copy to Output Directory** set to "Copy if newer". A missing or malformed file stops the game at startup with a
message that names it.

## 2. Recolor, keeping templates and states

Here is the whole file:

[!code-xml[](../../snippets/markup/guides/custom-themes/theme-extension.xml)]

The first style is the common case. `<Style TargetType="Button" Background="..."/>` in the file builds on the built-in
button style: it keeps the template and the focus state, and changes only the colors it sets. The built-in hover and press
states set colors of their own, so the file repeats them in teal (step 3); without that, the button would flash back to
the built-in grey when hovered or pressed. This is the rule a style in a page's resources follows over the theme too ([The theme underneath](../concepts/resources-and-styles.md#the-theme-underneath)),
applied to the theme file.

## 3. Replace a visual state group

A style that declares a state group with the same name as one of the built-in style's **replaces** that group, so it
has to repeat the states to keep. The list rows in the file replace `CommonStates` to hover and select in teal, and
keep the keyboard cursor state (`Highlighted`).

## 4. Give one control a new template

The check box in the file doesn't recolor the built-in one, it replaces it: it has its own template, a square with no
check mark, and its own colors and states. `ReplacesTheme="True"` tells IcyUI to drop the built-in style for that type
entirely, so the style provides everything the control needs: the template, the colors, and the states. This check box
has no hover or focus look, since the file doesn't declare those; add them the way the default theme does.

An entry in a theme file can use the ones declared before it: the style uses `GameCheckBoxTemplate` by key, so declare
the template first.

> [!NOTE]
> The built-in theme's own file is the best reference for what a template and its states look like:
> [DefaultTheme.xml](https://github.com/IOExcept10n/IcyUI/blob/platform-independent/sources/IcyUI/Resources/Themes/DefaultTheme.xml).

## 5. Replace the whole theme

To start from scratch, `UseThemeFile` makes a file the whole theme, instead of layering it:

# [Stride](#tab/stride)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/CustomThemesStride.cs#whole-theme)]

# [MonoGame](#tab/monogame)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/Guides/CustomThemesMonoGame.cs#whole-theme)]

---

Copy `DefaultTheme.xml` (linked above) into your project as `Themes/Dark.xml` and edit it. With `UseThemeFile` a
control the file doesn't style has no look at all: no template, no colors. If you only want to change some, prefer
`ExtendDefaultTheme`.

## What you built

A theme file that recolors the built-in buttons and lists, replaces the check box with a template of your own, and
leaves every other control as it was, plus the way to replace the built-in theme altogether.

> [!NOTE]
> A theme is chosen at startup: a canvas takes the theme when it's created, so changing it while the game runs doesn't
> restyle existing canvases. Keyed entries in a theme file (`x:Key`) are also not visible to `{StaticResource}` in your
> pages, which search only the page being loaded; use implicit styles for a theme, and keep keyed styles in a page's
> `Resources`.

## Next

- [Custom controls](custom-controls.md): give your own control a default look in this theme file.
- [Resources, styles and themes](../concepts/resources-and-styles.md): which value wins when several apply.
