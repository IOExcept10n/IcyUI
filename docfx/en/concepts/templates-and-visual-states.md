# Control templates and visual states

A control's behaviour lives in its class; its look lives in its template. A <xref:Icy.UI.Styles.ControlTemplate> is a
small tree of elements, built for each control that uses it, so you can give a button any look without changing how it
works. Visual states change properties when the control's state changes: hovered, pressed, focused, checked.

## A template

[!code-xml[](../../snippets/markup/concepts/template.xml)]

- `TargetType` says which control the template is for.
- `{TemplateBinding Background}` passes the control's own `Background` into the template, so a style or a local value on
  the button still reaches the border inside it.
- A `ContentPresenter` with `Content="{TemplateBinding Content}"` shows the control's content (here, the text "Play").

A template is usually set by a style (`Template="{StaticResource FlatButton}"`), so every button using the style gets
it. See [Resources, styles and themes](resources-and-styles.md).

## Named parts

Some controls look for named elements in their template and drive them: a slider moves its `PART_Thumb`, a progress bar
sizes its `PART_Fill`. A template for those controls must contain the parts by name, or that behaviour is missing.

| Control | Parts |
|---|---|
| `Slider` | `PART_Thumb` |
| `ProgressBar` | `PART_Fill` |
| `SplitPane` | `PART_Divider` |
| `Expander` | `PART_Header` |
| `TreeViewItem` | `PART_Indent`, `PART_Expander` |
| `Dropdown` | `PART_ToggleButton` |

## Visual states

A style's `StateGroups` hold <xref:Icy.UI.Styles.VisualStateGroup>s; each <xref:Icy.UI.Styles.VisualState> names the
control states it matches and the property values to use while they're active:

```xml
<VisualState Name="Hovered" State="Hovered" Background="#FF5A87E6" Duration="0:0:0.15" />
```

- `State` is one or more of `Normal`, `Hovered`, `Pressed`, `Disabled`, `Focused`, `Touching`, `Checked`, `Expanded`,
  `Selected`, `Highlighted` and `Invalid`, comma-separated: `State="Selected,Hovered"` matches only when both are on.
- In each group, the state matching the most current flags wins; when none matches, the style's own values apply.
- `Duration` (and `Easing`) animate the change instead of switching instantly.

A local value beats a visual state: a button with `Background="Red"` set on it doesn't change color on hover.

## Pitfalls

- **A template replaces the whole look.** Everything the theme's template drew (border, padding, focus outline) is gone
  unless your template draws it.
- **Forgetting `ContentPresenter`.** Without it, a button's text or icon isn't shown.
- **Missing parts.** A slider template without `PART_Thumb` draws, but has no thumb to drag.

## See also

- [Resources, styles and themes](resources-and-styles.md).
- [Animations](animations.md) for animating from code.
- The theme's templates in `DefaultTheme.xml` are complete examples for every control.
