# Markup editing and design tools

The `IcyUI.Design` package lets you edit your game's markup while the game runs: select an element on screen, move it,
change its properties, add controls, and save the result back to your `.xml` files. It's a development tool. Reference
it in Debug builds only; a shipped game doesn't need it and doesn't pay for it.

## What's in it

| Piece | What it does |
|---|---|
| <xref:Icy.Design.DesignSession> | Tracks every markup page the configuration loads once it's attached, one document per page. |
| <xref:Icy.Design.DesignDocument> | One page's markup text, linked to the elements built from it. An edit changes the text and the live elements together, can be undone, and is saved with `Save`. |
| <xref:Icy.Design.Editor.EditorOverlay> | The in-game editor over your running UI: Outline and Toolbox on the left, Properties on the right, a command bar on top. F4 shows and hides it. |
| <xref:Icy.Design.Editor.EditorWorkspace> | The same editor as a page, with a document picker and a preview of the open page, for a debug menu. |
| <xref:Icy.Design.Editor.EditorSession>, <xref:Icy.Design.Editor.EditorFrame> | The parts the two editors are built from: selection and edits, and the selection frame. For your own tools. |

## Setting it up

Add the package and make the reference Debug-only, so release builds don't carry it. After
`dotnet add package IcyUI.Design --prerelease`, add `Condition="'$(Configuration)' == 'Debug'"` to the
`PackageReference` it created.

Then attach a design session and the editor at startup, inside `#if DEBUG`:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/DesignTools.cs#attach-editor)]

- **Attach before loading screens.** The session sees only pages loaded after it was attached.
- **Point it at your source files.** A page is loaded from the build output, so without `UseSourceRoot`, saving would
  overwrite the copy there and your change would be lost at the next build. `FindSourceRoot` walks up from the game's
  folder to your project and returns the folder your markup lives in.

For a debug menu instead of an overlay, put an `EditorWorkspace` on one of its pages:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/DesignTools.cs#workspace)]

A canvas takes one editor at a time: while the overlay is shown, the workspace waits, and the other way round.

## Editing

The editor has two modes, switched with Ctrl+Shift+E or the command bar: in **Edit** mode a click selects the element
under the pointer; in **Interact** mode the UI works as in the game, so you can open a menu or a tab and then edit
what it shows.

- **Select** with a click, or in the Outline. Alt+arrows move the selection to the parent, the first child or a sibling.
- **Move and resize** by dragging the selection or its handles. In a `Grid` an element drops into the cell under the
  pointer and resizes by row and column spans; in a `StackPanel` it drops into the nearest gap between its siblings;
  elsewhere it moves by its margin. Arrow keys nudge it (Shift for a larger step); Delete removes it.
- **Change properties** in the Properties panel. It shows the values on screen (from styles and bindings too), and an
  edit sets the attribute in the markup. Quick successive edits of one property undo as one step.
- **Add controls** by clicking them in the Toolbox: into the selected element when it's a panel or an empty content
  control, otherwise right after it.
- **Undo and redo** with Ctrl+Z and Ctrl+Y (or Ctrl+Shift+Z). Each document has its own history.
- **Save** or **Save all** from the command bar. Saving writes the document's text as it is: your formatting and
  comments stay.

Most edits show at once. An edit inside a style, a template or a property element (`<Button.Content>`) changes the
text but not the live page; the document reports `NeedsReload`, and the change shows the next time the page loads.

Editing the `.xml` file in your text editor while the game runs works too: `ReloadFromSource()` on its document reads
the file and applies the text, as a single edit you can undo.

## Performance

Nothing of `IcyUI.Design` runs unless you attach it, and with the Debug-only reference a release build doesn't contain
it at all. While attached, the session keeps each tracked page's text and a map from its markup to its elements. Hiding
the overlay detaches its frame and panels; the panels do no per-frame work, they refresh when a document changes.

## Pitfalls

- **An element can't be selected.** It was created in code, or its page was loaded before the session was attached.
  Only elements built from tracked markup can be edited.
- **Save is disabled.** The document doesn't know which file to write: set `UseSourceRoot`, or a `SourcePathResolver`
  that maps your asset paths to files. The command bar shows the reason.
- **A change doesn't show.** It's inside a style or a template; reload the page.

## See also

- [Markup reference](../markup/index.md).
- <xref:Icy.Design.DesignSession> and <xref:Icy.Design.Editor.EditorOverlay> in the API reference.
