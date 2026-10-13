# Tabs and navigation

![A tab control with Video, Audio and Controls tabs, Video selected and showing its resolution](../../images/controls/navigation.png)

A <xref:Icy.UI.Controls.TabControl> shows one page at a time under a row of tabs, for example the sections of a
settings screen. A <xref:Icy.UI.Controls.Frame> shows one <xref:Icy.UI.Controls.Page> at a time and navigates between
pages, with back and forward history, for whole screens of a menu.

## Markup

[!code-xml[](../../snippets/markup/controls/navigation.xml)]

The tabs come from `ItemsSource`: a list of <xref:Icy.UI.Controls.TabItem>s, each with a header and the page it shows.
Here the view model provides them:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Controls.cs#tabs)]

## Key properties

| Property | Description |
|---|---|
| <xref:Icy.UI.Controls.ItemsControl.ItemsSource> | The `TabItem`s. |
| <xref:Icy.UI.Controls.SelectingItemsControl.SelectedIndex> | The open tab. |
| <xref:Icy.UI.Controls.TabItem.Header> / <xref:Icy.UI.Controls.TabItem.Icon> | A tab's label and icon. |
| <xref:Icy.UI.Controls.ContentControl.Content> | A tab's page (on the `TabItem`). |
| <xref:Icy.UI.Controls.Frame.Navigation> | A frame's navigation: the current page and its history. |

## Events and commands

<xref:Icy.UI.Controls.SelectingItemsControl.SelectionChanged> fires when the open tab changes.

## Styling

Tab headers are `SelectorItem`s, styled like list items (`Hovered`, `Selected`, ...). The tab control's own style sets
its background and border.

## Keyboard, gamepad and touch

A click or tap on a tab opens it.

> [!NOTE]
> Tabs can't be switched with the keyboard or a gamepad yet.

## TabControl

Keeps only the open tab's page in the layout.

## Frame and Page

A frame navigates between pages described in markup; see [Pages and navigation](../markup/index.md#pages-and-navigation)
in the markup reference.

## See also

- [Lists and selection](lists.md): a tab control is a list of tabs with a selection.
