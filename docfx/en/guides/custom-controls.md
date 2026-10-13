# Custom controls

This guide builds `HealthBar`, a control for a game's HUD: it shows how much health is left, has its own typed
properties, can be used from markup like any built-in control, and turns red below a quarter. Along the way you'll
see what a control needs to behave like the built-in ones: properties markup and bindings can reach, layout, a
replaceable template, and states a theme can style.

You need a game with IcyUI set up ([Getting started](../getting-started/stride.md)) and a feel for
[templates and visual states](../concepts/templates-and-visual-states.md).

> [!TIP]
> Many "custom controls" don't need code. A <xref:Icy.UI.Controls.ProgressBar> with a style and a template already makes
> a good health bar. Write a control when it has its own behaviour or properties your game code works with.

## 1. Derive from `Control`

Derive from <xref:Icy.UI.Controls.Control> for something with a background, a border and a template, or from
<xref:Icy.UI.UIElement> for something you draw and lay out yourself. `HealthBar` is a `Control` that holds a fill inside
its border:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/HealthBar.cs#health-bar)]

Four things make it a good citizen:

- **Properties.** `[RegisterReference]` makes a property reachable by markup attributes, styles, bindings and the design
  tools; no registration call is needed. `SetProperty` raises `PropertyChanged`, which is how bindings and styles notice
  a change. `[Category]` and `[DefaultValue]` document it for the editor.
- **Layout.** `MeasureContent` reports the natural size (`HealthBar` is 160 × 18 unless the page gives it a size), and
  `ArrangeContent` lays out the parts once the final size is known. `[AffectsArrange]` (or `[AffectsMeasure]`) tells the
  property system a change needs a new layout pass; `InvalidateArrange()` does it from code.
- **A replaceable template.** A theme or a page can give the control a `Template` (see step 3). The control keeps a
  reference to the part it moves, the fill, so it overrides <xref:Icy.UI.Controls.Control.OnApplyTemplate> to find the
  template's `PART_Fill` with <xref:Icy.UI.Controls.Control.GetTemplateChild*>, or to go back to its
  own fill when there's no template.
- **States.** A theme styles a control through its <xref:Icy.UI.UIElement.ControlState>. The flags are fixed
  (<xref:Icy.UI.Styles.ControlState>): a custom control picks the one that means what it needs. `HealthBar` uses
  `Highlighted` for "critical" and keeps the flags the input system sets, such as `Hovered`.

## 2. Use it from markup

Markup finds a type through a `clr-namespace` prefix, with the assembly name of your game:

[!code-xml[](../../snippets/markup/guides/custom-controls/health-hud.xml)]

`Value="{Binding Health}"` binds the bar to the HUD's view model, as for any other control. To drop the prefix,
register a short name once at startup with <xref:Icy.Markup.MarkupConfiguration.RegisterShortName*>, and write
`<HealthBar/>`. The `clr-namespace` form always works; short names only remove the prefix.

## 3. Give it a default look in your theme

A control has no look in the built-in theme, only the plain fill from its constructor. Your game's theme gives it a
template and the states. The theme is a file you layer over the built-in one with
<xref:Icy.Configuration.ThemeBuildingExtensions.ExtendDefaultTheme*>; see the Custom themes guide:

[!code-xml[](../../snippets/markup/guides/custom-controls/health-theme.xml)]

- The template's `PART_Fill` is the part the control looks for. A template without one still applies, but the bar
  shows no fill.
- `State="Highlighted"` is the state `HealthBar` sets below a quarter; the style changes only `FillBrush`.
- The file names the type with a `clr-namespace` prefix, not a short name, because a theme file is loaded while the
  configuration is built, before your startup code can register the short name.

Load the theme and the page when the game starts:

# [Stride](#tab/stride)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/HealthBarStride.cs#register)]

# [MonoGame](#tab/monogame)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/Guides/HealthBarMonoGame.cs#register)]

---

Set `Health` on the view model from your game code and the bar follows; the fill goes red below 25.

## What you built

A control with typed properties that markup, styles and bindings can use, that lays itself out, works with or without a
template, and exposes a state the theme styles, without a line of drawing code.

> [!NOTE]
> A custom control can't add new visual state flags yet: use the closest existing one and say what it means in your
> theme, as `HealthBar` does with `Highlighted`.

## Next

- Custom themes: recolor the built-in controls and add your own templates.
- [Templates and visual states](../concepts/templates-and-visual-states.md): the concepts behind steps 1 and 3.
- [Markup reference](../markup/index.md): namespaces and short names.
