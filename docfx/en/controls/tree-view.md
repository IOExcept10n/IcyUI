# Tree view

![A Settings tree with Video, Audio and an expanded Controls node holding Keyboard and Gamepad](../../images/controls/tree-view.png)

A <xref:Icy.UI.Controls.TreeView> shows nested items that the player expands and collapses: a settings tree, a quest
log, a scene hierarchy in a level editor.

## Markup

[!code-xml[](../../snippets/markup/controls/tree-view.xml)]

A static tree is written in markup with <xref:Icy.UI.Controls.TreeViewNode> elements: each node has a `Header`, its
child nodes, and `IsExpanded`/`IsSelectable`. For a tree built from your data, bind
<xref:Icy.UI.Controls.TreeView.ItemsSource> to the root items and set <xref:Icy.UI.Controls.TreeView.ChildrenSelector>
to a function that returns an item's children.

## Key properties

| Property | Description |
|---|---|
| <xref:Icy.UI.Controls.TreeView.Items> | The inline root nodes written in markup. |
| <xref:Icy.UI.Controls.TreeView.ItemsSource> / <xref:Icy.UI.Controls.TreeView.ChildrenSelector> | A tree built from your data. |
| <xref:Icy.UI.Controls.TreeView.SelectedItem> | The selected item. |
| <xref:Icy.UI.Controls.TreeView.Indent> | How far each level is indented. |

## Events and commands

<xref:Icy.UI.Controls.TreeView.SelectionChanged>, <xref:Icy.UI.Controls.TreeView.ItemExpanded> and
<xref:Icy.UI.Controls.TreeView.ItemCollapsed>.

## Styling

Each row is a `TreeViewItem` whose template has the parts `PART_Indent` and `PART_Expander` (the chevron). Its visual
states are `Hovered`, `Selected`, `SelectedHovered`, `Highlighted`, `HighlightedHovered`, `SelectedHighlighted` and
`Disabled`.

## Keyboard, gamepad and touch

Up and Down (or the D-pad) move between rows. Right expands a row, or moves into it if it's already expanded; Left
collapses it, or moves to its parent. Enter or the gamepad's A button toggles a row with children. A click or tap on the
chevron expands or collapses; on the row, it selects.

## See also

- [Lists and selection](lists.md) for flat lists.
