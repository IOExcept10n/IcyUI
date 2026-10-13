# Layout

Layout decides where every element sits and how big it is. Each frame where something changed, IcyUI asks every element
how big it wants to be, then gives it a rectangle to fill. Panels decide how their children share their space; each
child's size, margins and alignment decide where it sits in the share it gets.

## Measure, then arrange

Layout runs in two passes, from the canvas down:

1. **Measure:** each element reports its desired size, from its content (text, an image, its children) and its own
   size properties.
2. **Arrange:** each parent gives each child a rectangle. The child places itself in it according to its margin and
   alignment, and arranges its own children.

Changing a property that affects size (text, font size, margins, children) marks the element for a new measure; the
next frame lays it out again. You don't call layout yourself.

## Size

| Property | Meaning |
|---|---|
| <xref:Icy.UI.UIElement.Width> / <xref:Icy.UI.UIElement.Height> | A fixed size. Unset by default (`NaN`), meaning "size to content, or to the space given". |
| <xref:Icy.UI.UIElement.MinWidth> / <xref:Icy.UI.UIElement.MaxWidth> (and heights) | Limits on the size the element ends up with. |
| <xref:Icy.UI.UIElement.Margin> | Space outside the element, between it and its neighbours or parent. |
| <xref:Icy.UI.UIElement.Padding> | Space inside the element, between its border and its content. |

A `Thickness` is written in whole units: `"8"` (all sides), `"8,4"` (left/right, top/bottom) or `"8,4,8,0"` (left, top, right,
bottom).

## Alignment

Inside the rectangle it's given, an element is placed by <xref:Icy.UI.UIElement.HorizontalAlignment> (`Left`,
`Center`, `Right`, `Stretch`) and <xref:Icy.UI.UIElement.VerticalAlignment> (`Top`, `Center`, `Bottom`, `Stretch`).
`Stretch` fills the rectangle unless a `Width`/`Height` is set; the others size the element to its content.

[!code-xml[](../../snippets/markup/concepts/layout.xml)]

Elements stretch by default. The default theme centers buttons, so a button added straight to a canvas doesn't cover
the game; a local `HorizontalAlignment="Stretch"` on a button fills its slot again.

## Panels

| Panel | How it shares its space |
|---|---|
| `StackPanel` | One child after another, in a column or a row. |
| `Grid` | Rows and columns sized `Auto`, in pixels or as shares (`*`, `2*`); children span cells. |
| `WrapGrid` | Equal cells in rows that wrap, for inventories. |
| `ScrollViewer` | Content larger than itself, scrolled. |
| `SplitPane` | Two panes with a draggable divider. |

See [Panels](../controls/panels.md) and [Scrolling](../controls/scrolling.md) for each one's properties. A list (a
`ListBox` or `WrapGrid`) outside a `ScrollViewer` sizes to its items.

## Scale

Layout works in UI units. The canvas multiplies them by its scale (the display's DPI, or a reference resolution) when
it draws, so a 200-unit-wide panel stays the same physical size on a 4K screen. See [DPI and UI scaling](scaling.md).

## Pitfalls

- **Limits are `NaN` when unset.** In your own controls, don't pass `Width` or `MinWidth` straight to `Math.Min`,
  `Math.Max` or `float.Clamp`: a `NaN` bound gives `NaN`. Check `float.IsNaN` first.
- **A stretched child in a sized-to-content parent has nothing to stretch to.** In a `StackPanel` row, `Stretch` along
  the row means "size to content"; give the child a `Width` or use a `Grid` with a `*` column.
- **Hidden elements keep their space.** See [Canvas, the host and the element tree](canvas-and-host.md#pitfalls).

## See also

- [Panels](../controls/panels.md), [Scrolling](../controls/scrolling.md).
- [DPI and UI scaling](scaling.md).
