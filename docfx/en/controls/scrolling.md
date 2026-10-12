# Scrolling

![A scroll viewer showing the first eight of a list of levels](../../images/controls/scrolling.png)

A <xref:Icy.UI.Controls.ScrollViewer> shows content larger than itself and scrolls it: a long list, a credits screen, a
codex page.

## Markup

[!code-xml[](../../snippets/markup/controls/scrolling.xml)]

Give the scroll viewer a size (or put it where its parent limits it); its content can then be as tall as it needs.

## Key properties

| Property | Description |
|---|---|
| <xref:Icy.UI.Controls.ScrollViewer.VerticalScrollMode> / <xref:Icy.UI.Controls.ScrollViewer.HorizontalScrollMode> | Whether it scrolls along each axis. |
| <xref:Icy.UI.Controls.ScrollViewer.VerticalOffset> / <xref:Icy.UI.Controls.ScrollViewer.HorizontalOffset> | The scroll position. |
| <xref:Icy.UI.Controls.ScrollViewer.PanningMode> | Which axes pan with touch and the middle mouse button. |

## Events and commands

<xref:Icy.UI.Controls.ScrollViewer.ScrollChanged> fires when the position changes.

## Styling

The default theme draws no frame; set `BorderThickness` and `BorderBrush` for one.

## Keyboard, gamepad and touch

The mouse wheel scrolls. A finger, or the middle mouse button, drags the content, and a flick keeps it moving for a
moment. Moving focus to an element out of view scrolls it into view.

> [!NOTE]
> MonoGame's DesktopGL backend reports no touch input, so panning there works with the middle mouse button only.

## ScrollViewer

With a list or grid of items inside, only the items in view are created, so even very long lists scroll smoothly.

## See also

- [Lists and selection](lists.md).
