# Assets and fonts

IcyUI loads markup files, resource dictionaries, fonts and images through one asset pipeline: you ask for a file by
path and a type, and the importer for that type reads it. Fonts have their own system on top, which finds a font by
family name, imports it when first needed, and falls back when it can't.

## Loading files

`Configuration.Assets.Load<T>(path)` loads a file and returns it as `T`:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Concepts.cs#load-asset)]

- **Where:** paths are relative to the configuration's `DefaultAssetContext`: the game's output folder unless you change
  it. Set a file's **Copy to Output Directory** to "Copy if newer" so it's there at run time.
- **What:** with the engine defaults, `UIElement` (markup), `ResourceDictionary` (a resource-dictionary file) and fonts
  load out of the box, and the engine integration adds textures. `LoadAsync<T>` loads without blocking.
- **Shortcut:** `Canvas.LoadMarkup(path, viewModel)` loads markup, sets its data context and adds it to the canvas.

A file that doesn't exist throws; a markup file with an error throws a `MarkupException` naming the file, line and
column.

## Fonts

Text asks for a font by family, size and style. The <xref:Icy.Rendering.Fonts.FontSystem> resolves it:

1. a font already imported, at any size (fonts are vector and render at any size);
2. a **system font** from the Windows fonts folder, when system fonts are enabled (they are with the engine defaults);
   the folder is indexed once, the first time a family isn't found among the imported ones;
3. the **fallback font**, when nothing else matches. IcyUI logs a warning once for that family (event 1001), and text
   using it draws in the fallback font, or not at all if there's none.

Text without a `FontFamily` uses <xref:Icy.Rendering.Fonts.FontSystem.DefaultFontFamily>: Segoe UI with the engine
defaults, or the family you set with `WithDefaultFontFamily`.

## Your own font

Ship a font file with the game and import it during setup, then make it the default:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Fonts.cs#own-font)]

The family name is the one stored in the font file, not the file name. Import a file once and use it at every size.

Don't copy Windows fonts into your game: their license doesn't allow redistribution. Fonts under the SIL Open Font
License, such as Noto Sans or Inter, can be shipped with any game. On Linux and macOS there is no Segoe UI, so a game
for those platforms should always import its own font.

## Pitfalls

- **Text doesn't appear.** Check the log for a font warning (1001): the family name is misspelled, or the font isn't
  installed or imported. See [Logging and diagnostics](logging-and-diagnostics.md).
- **Files missing at run time.** A file that isn't copied to the output folder isn't found when the game runs; check
  its Copy to Output Directory setting.
- **Loading every frame.** `Load` reads and parses the file each time (unless you've opened a cache with
  `AssetResolver.CreateScope`); load once and keep the result.

## See also

- [Fonts on other platforms](../getting-started/stride.md#fonts-on-other-platforms).
- [Text](../controls/text.md), [Images and icons](../controls/images.md).
