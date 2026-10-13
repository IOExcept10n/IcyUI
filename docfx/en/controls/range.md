# Range controls

![A Volume slider at 70 and a Loading progress bar at 40 percent](../../images/controls/range.png)

A <xref:Icy.UI.Controls.Slider> lets the player pick a value in a range by dragging its thumb: volume, sensitivity,
field of view. A <xref:Icy.UI.Controls.ProgressBar> shows a value in a range: loading, health, experience.

## Markup

[!code-xml[](../../snippets/markup/controls/range.xml)]

Bind `Value` to your view model to read or drive it: `Value="{Binding Volume, Mode=TwoWay}"`.

## Key properties

| Property | Description |
|---|---|
| <xref:Icy.UI.Controls.Slider.Minimum> / <xref:Icy.UI.Controls.Slider.Maximum> | The range. |
| <xref:Icy.UI.Controls.Slider.Value> | The current value, kept inside the range. |
| <xref:Icy.UI.Controls.Slider.ThumbBrush> | The thumb's color. |
| <xref:Icy.UI.Controls.ProgressBar.Value> | The progress, between <xref:Icy.UI.Controls.ProgressBar.Minimum> and <xref:Icy.UI.Controls.ProgressBar.Maximum>. |
| <xref:Icy.UI.Controls.ProgressBar.FillBrush> | The filled part's color. |

## Events and commands

<xref:Icy.UI.Controls.Slider.ValueChanged> fires while the player drags.

## Styling

The slider's template has a part named `PART_Thumb`, and its visual states are `Hovered` (a lighter thumb) and
`Focused`. The progress bar's template has a part named `PART_Fill`, sized to the value.

## Keyboard, gamepad and touch

The slider follows the mouse or a finger while dragged. It takes focus, so the arrow keys and the D-pad can reach it.

> [!NOTE]
> The arrow keys and the D-pad don't change a slider's value yet; they move focus past it.

## Slider

Values outside the range are clamped to it.

## ProgressBar

Display only: it doesn't take focus or input.

## See also

- [Lists and selection](lists.md) for choosing from a set of values instead.
