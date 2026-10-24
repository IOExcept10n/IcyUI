# Images and icons

![Five icons: a check mark and four chevrons](../../images/controls/images.png)

An <xref:Icy.UI.Controls.Icon> draws a small vector glyph, such as a check mark or an arrow, at any size and in any
color. An <xref:Icy.UI.Controls.Image> shows a picture: a texture loaded through your engine.

## Markup

[!code-xml[](../../snippets/markup/controls/images.xml)]

## Key properties

| Property | Description |
|---|---|
| <xref:Icy.UI.Controls.Icon.Kind> | Which glyph: `Checkmark`, `ChevronUp`, `ChevronDown`, `ChevronLeft` or `ChevronRight` (<xref:Icy.UI.Controls.IconKind>). |
| <xref:Icy.UI.Controls.Icon.Stroke> | The glyph's line color. |
| <xref:Icy.UI.Controls.Icon.StrokeThickness> | The glyph's line width. |
| <xref:Icy.UI.Controls.Image.Source> | The picture to show. |

An icon scales its glyph to its own size, so give it a `Width` and `Height`.

## Events and commands

Neither raises events of its own. To make an icon clickable, put it inside a [button](buttons.md).

## Styling

Icons and images have no template or visual states; set their properties directly or in a `Style`.

## Keyboard, gamepad and touch

Neither takes focus or input.

## Icon

The five glyphs are drawn as strokes, so they stay sharp at any size and scale.

## Image

Shows its <xref:Icy.UI.Controls.Image.Source>, an image loaded through the configuration's assets (the engine
integrations import textures). In markup, set it to the texture's path: `<Image Source="Portraits/Warrior.png"/>`. The
path is relative to the same folder as page and dictionary paths, and every element with the same path shares one
texture.

A binding needs an image rather than a path: bind `Source` to a view-model property of type `IImage`, such as an
`ImageBrush` around a texture from `Configuration.Assets.Load<ITexture>(path)`.

### Pixel art

Scaled images are smoothed by default. For pixel art, set `TextureFiltering="Point"` and each pixel stays a hard-edged
block at any size. Set it once on the canvas (`canvas.TextureFiltering = TextureFiltering.Point`) to cover every image,
nine-patch frame and pixel font on it, or on one element to cover that element and everything inside it. An element set
back to `Linear` keeps a smooth image inside a pixel-art screen. Each place where the filtering changes between
neighbouring elements costs the engine a separate draw batch, so prefer one setting for a whole screen.

## See also

- [Buttons](buttons.md), whose content can be an icon.
