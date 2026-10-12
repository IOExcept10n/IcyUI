# Resources, styles and themes

A style sets a group of properties on every control it applies to, so a look is defined once and reused. Resources hold
styles, templates, brushes and any other value under a key. A theme is a resource dictionary of styles for every
control; IcyUI ships one, and you can replace or extend it.

## Resources

Every element has a <xref:Icy.UI.UIElement.Resources> dictionary. `{StaticResource Key}` looks a key up in the element's
own resources, then in each enclosing element of the same markup document, and uses the first match. It's resolved
once, while the document loads, so declare `Resources` before the children that use them; a key that isn't found is a
markup error.

A dictionary can merge others (`MergedDictionaries`), including resource-dictionary files loaded through the asset
pipeline. See [Resources and styles](../markup/index.md#resources-and-styles) in the markup reference for the syntax.

## Styles

A <xref:Icy.UI.Styles.Style> has a `TargetType`, property values (its attributes in markup), and optional visual
states.

- **Implicit** (no `x:Key`): applies to every control of exactly that type, in the element that declares it and below,
  that has no style of its own.
- **Keyed** (`x:Key`): applies where a control names it, `Style="{StaticResource Key}"`.
- **`BasedOn`**: a style can start from another one's values and change some of them.

[!code-xml[](../../snippets/markup/concepts/styles.xml)]

## The theme underneath

The theme's styles are the implicit styles of every built-in control, held by the canvas. A style of your own, keyed or
implicit, is applied **on top of** the theme's style for its type: the theme supplies the template, colors and visual
states, and your style changes only what it sets. In the example, `DangerButton` keeps the theme's button template and
changes only its background, padding and alignment.

To take the theme's look off a control entirely, set `ReplacesTheme="True"` on its style; the style then has to provide
everything itself, including a [template](templates-and-visual-states.md).

To change the whole look of your game, set your own theme in the configuration (`ConfigureTheme`), for example a copy of
the built-in `DefaultTheme.xml` with your colors.

## Which value wins

A property can get its value from several places. The highest one present wins:

1. a **local value**: set on the element in markup or code;
2. a running **animation**;
3. an active **visual state** (hovered, pressed, ...);
4. a **style** (yours, over the theme's);
5. the property's **default**.

So `<Button Background="Red"/>` stays red when hovered (local beats visual state), and a value set in code beats the
style. A local value equal to the default still counts: `HorizontalAlignment="Stretch"` on a button overrides the
theme's centered alignment.

## Pitfalls

- **Implicit styles match the exact type.** A `Button` style doesn't apply to a `ToggleButton`; give each type its own,
  or base one on the other.
- **`{StaticResource}` doesn't see the theme or the canvas.** It searches the document being loaded; reach theme values
  through implicit styles instead.
- **Text properties don't inherit.** `FontSize` or `Foreground` on a panel doesn't reach the text inside it; use an
  implicit `TextBlock` style in the panel's resources.

## See also

- [Control templates and visual states](templates-and-visual-states.md).
- [Resources and styles](../markup/index.md#resources-and-styles) in the markup reference.
- [Buttons](../controls/buttons.md) for the states each control's style uses.
