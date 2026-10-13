# Expander and split pane

![An expanded "Advanced" section above a split pane with Scene and Properties panes](../../images/controls/expander-split.png)

An <xref:Icy.UI.Controls.Expander> shows or hides its content under a header, for optional detail such as advanced
settings. A <xref:Icy.UI.Controls.SplitPane> divides its space between two panes with a divider the player drags, as
in an editor layout.

## Markup

[!code-xml[](../../snippets/markup/controls/expander-split.xml)]

The expander's content is its child element. The split pane's two panes are property elements, `SplitPane.First` and
`SplitPane.Second`.

## Key properties

| Property | Description |
|---|---|
| <xref:Icy.UI.Controls.Expander.Header> | The header: text or any element. |
| <xref:Icy.UI.Controls.Expander.IsExpanded> | Whether the content shows. |
| <xref:Icy.UI.Controls.SplitPane.First> / <xref:Icy.UI.Controls.SplitPane.Second> | The two panes. |
| <xref:Icy.UI.Controls.SplitPane.Orientation> | `Horizontal` puts the panes side by side; `Vertical` stacks them. |
| <xref:Icy.UI.Controls.SplitPane.SplitterPosition> | Where the divider sits, from 0 to 1. |
| <xref:Icy.UI.Controls.SplitPane.MinFirstSize> / <xref:Icy.UI.Controls.SplitPane.MinSecondSize> | How small each pane can get. |

## Events and commands

<xref:Icy.UI.Controls.Expander.IsExpandedChanged> and <xref:Icy.UI.Controls.SplitPane.SplitterPositionChanged>.

## Styling

The expander's template has a header part, `PART_Header` (an `ExpanderHeader`), with the visual states `Hovered`,
`Pressed`, `Focused` and `Checked` (the chevron turns). The expander animates between its `Expanded` and `Collapsed`
states over 0.2 seconds. The split pane's divider is the `PART_Divider` part, with `Hovered` and `Pressed` states.

## Keyboard, gamepad and touch

The expander's header is a toggle button: Enter or the gamepad's A button opens or closes it, and so does a click or
tap. The split pane's divider follows a mouse drag or a finger.

## Expander

Collapsed, it takes only its header's height.

## SplitPane

The panes resize as the divider moves; each stays at least its minimum size.

## See also

- [Panels](panels.md) for fixed layouts.
