# Animations

An animation changes a property over time: a panel fading in, a button growing on hover, a health bar sliding down.
IcyUI animates any registered property of an element, from code or from a style's visual states.

## From code

`Animate` starts a <xref:Icy.Animations.Timeline> on an element and returns the running
<xref:Icy.Animations.Animation>:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#animate)]

`Timeline.FromTo(property, duration, from, to, easing, repeatCount, autoReverse)` covers most cases:

- the property is named as a string, `nameof(UIElement.Opacity)` for example;
- numbers, colors, sizes and thicknesses interpolate between the two values;
- `repeatCount` plays it several times (`Timeline.Forever` until stopped), and `autoReverse` plays each pass forward
  then backward, for pulsing effects.

The returned animation can be paused, resumed and stopped, and raises `Completed` when it ends.

## Easing

Easing shapes the motion: `Easing.Linear` (constant speed), and the `EaseIn`, `EaseOut` and `EaseInOut` variants of
`Quad` and `Cubic`. Ease-out (fast start, gentle stop) suits elements arriving on screen; ease-in suits elements leaving
it.

## From visual states

A visual state with a `Duration` animates to its values instead of switching instantly, which is the usual way to make
hover and press feel smooth:

```xml
<VisualState Name="Hovered" State="Hovered" Background="#FF5A87E6" Duration="0:0:0.15" Easing="EaseOutCubic" />
```

See [Control templates and visual states](templates-and-visual-states.md).

## Animations and other values

A running animation outranks styles and visual states but not a local value (see
[Resources, styles and themes](resources-and-styles.md#which-value-wins)). A finished animation keeps holding its last
value; `Stop()` removes it, and the property returns to the value it would have without the animation. To keep the
final value for good, set it on the element and then stop the animation.

## Pitfalls

- **A local value hides the animation.** Animating `Opacity` on an element whose `Opacity` is set in its markup or in code
  changes nothing on screen; leave the property to a style, or animate a different one.
- **Layout properties animate layout.** Animating `Width`, `Margin` or `Padding` re-runs layout every frame for that
  element and its neighbours; prefer `Opacity` and the render transforms (`RenderScale`, `RenderRotation`) for effects that don't move other
  elements.
- **Animations run with the frame clock.** A paused game that stops updating IcyUI pauses its animations too.

## See also

- <xref:Icy.Animations.AnimationExtensions> and <xref:Icy.Animations.Timeline> in the API reference.
- [Control templates and visual states](templates-and-visual-states.md).
