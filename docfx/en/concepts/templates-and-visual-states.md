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

A style's `StateGroups` hold <xref:Icy.UI.Styles.VisualStateGroup>s; each <xref:Icy.UI.Styles.VisualState> says which
control states it matches and the property values to use while they're active:

```xml
<VisualState State="Hovered" Background="#FF5A87E6" Duration="0:0:0.15" />
```

- `State` is one or more of `Normal`, `Hovered`, `Pressed`, `Disabled`, `Focused`, `Touching`, `Checked`, `Expanded`,
  `Selected`, `Highlighted` and `Invalid`, comma-separated: `State="Selected,Hovered"` matches only when both are on.
- The control sets these flags itself, and several are often on at once: a mouse press keeps `Hovered` on, and a
  button you tabbed to stays `Focused` while you hover it.
- `Duration` (and `Easing`) animate the change instead of switching instantly.

A local value beats a visual state: a button with `Background="Red"` set on it doesn't change color on hover.

### Which state is active

**Each group has at most one active state.** It's the state that matches the most of the control's current flags; when
two match equally many, the one declared **last** wins. So list states from the least to the most important:
`Hovered`, then `Pressed`, and a press shows the pressed color even though the button is still hovered. When no state
matches, the style's own values apply.

### Why several groups

Because a group has one active state, states that should **combine** belong in different groups. A focus border and a
hover background are independent, so the theme keeps them apart:

[!code-xml[](../../snippets/markup/concepts/state-groups.xml)]

In one group, hover and focus would compete, and a focused button would lose its border as soon as the pointer moved
over it. In two groups, each has its own active state. Use one group for states that set the **same** property and
replace each other (hovered, pressed), and a separate group for each independent aspect: focus, checked, enabled.
Every interactive control in the default theme has an `EnabledStates` group whose `Disabled` state halves its
`Opacity`; it combines with whatever the other groups show.

When active states of different groups set the same property, the group declared later wins; when its state ends, the
earlier group's value comes back.

### What names are for

A **group's** `Name` is how a style overrides inherited states. Your style is applied over the theme's style for the
same control, and over its `BasedOn` style; a group with the same name as one of theirs **replaces** it. The snippet
above names its group `CommonStates`, as the theme does, so the theme's hover and pressed colors are gone and the
theme's `FocusStates` group stays. A group with a new name, or no name, is added alongside theirs.

A **state's** `Name` is optional and changes nothing: it's a label for readers, the design tools and diagnostics.

## Pitfalls

- **A template replaces the whole look.** Everything the theme's template drew (border, padding, focus outline) is gone
  unless your template draws it.
- **Forgetting `ContentPresenter`.** Without it, a button's text or icon isn't shown.
- **Missing parts.** A slider template without `PART_Thumb` draws, but has no thumb to drag.
- **Independent states in one group.** Focus, hover and checked in the same group take turns instead of combining.
- **Accidentally replacing a theme group.** Naming your group `CommonStates` drops all the theme's common states, not
  just the ones you redefine. Repeat the ones you want to keep, or give your group another name.

## See also

- [Resources, styles and themes](resources-and-styles.md).
- [Animations](animations.md) for animating from code.
- The theme's templates in `DefaultTheme.xml` are complete examples for every control.
