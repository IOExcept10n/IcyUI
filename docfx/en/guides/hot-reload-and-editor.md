# Hot reload and the in-game editor

This guide sets up the edit loop for a game's UI: change a page's markup in your text editor, or in an editor over the
running game, and see the result without restarting. It uses the `IcyUI.Design` package, which is a development tool;
a shipped game doesn't contain it.

You need a game that loads a page from markup ([Your first screen](../getting-started/first-screen.md)). The
[Markup editing and design tools](../concepts/design-tools.md) page describes what the tools are; this guide wires
them up and shows how to work with them.

## 1. Reference the package in Debug builds only

Add the package, then make the reference conditional so a release build doesn't carry it:

```xml
<ItemGroup Condition="'$(Configuration)' == 'Debug'">
  <PackageReference Include="IcyUI.Design" Version="0.1.0-alpha.1" />
</ItemGroup>
<!-- The code that uses it lives in a DevTools folder, compiled in Debug only. -->
<ItemGroup Condition="'$(Configuration)' != 'Debug'">
  <Compile Remove="DevTools\**" />
</ItemGroup>
```

Pages are loaded from the build output (`bin/Debug/.../Menu.xml`), a copy of the file in your project. Editing the
copy would be lost at the next build, so the tools need to know where your project's files are.

## 2. Attach the tools

Put this class in the `DevTools` folder. It attaches a design session, points it at your source files, watches them,
and adds the editor:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/HotReload.cs#dev-tools)]

- **Attach before the first page loads.** The session tracks only the pages loaded after it was attached.
- **`UseSourceRoot` is what makes saving and watching work.** `FindSourceRoot` walks up from the game's output folder
  to the folder containing `MyGame.csproj` and returns `Assets/UI` inside it, which is where the markup lives. If it
  finds nothing, saving and watching use the copy in the build output, which the next build overwrites.
- **`WatchSourceFiles` is opt-in.** Nothing is watched unless you call it.

## 3. Edit in your text editor

With the game running, save the `.xml` file in your project. Within a fraction of a second the live page changes: a
button's `Width`, a text, a color, an added control. Behind the scenes the tools read the file and apply it to the page
as one edit.

- It also works for editors that save by writing a temporary file and renaming it.
- Several saves in quick succession apply once.
- A file whose text equals what the game already has is ignored. That is why saving from the in-game editor (step 4)
  doesn't reload itself.

Not every edit can change the live page. An edit inside a `Style`, a `ControlTemplate` or a property element
(`<Button.Content>`) changes the document but not the elements already on screen; the document reports `NeedsReload`.
Load the page again to see it. `ReloadablePage` does that on **F5**, from the document's latest text:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/HotReload.cs#reload-page)]

It builds the page from the document's text and not with `LoadMarkup`, because `LoadMarkup` would read the older copy in
the build output.

Now start the game with the tools:

# [Stride](#tab/stride)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Guides/HotReloadStride.cs#design-setup)]

# [MonoGame](#tab/monogame)

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/Guides/HotReloadMonoGame.cs#design-setup)]

---

In a Release build the `#else` branch loads the page the normal way, and the tools aren't compiled at all.

## 4. Edit in the game

Press **F4** to show the editor over the running game. Click an element to select it, change its properties in the
panel on the right, drag it, or add controls from the Toolbox. **Ctrl+Shift+E** switches between Edit mode (a click
selects) and Interact mode (the UI works as in the game, so you can open a menu and edit what it shows). **Ctrl+Z** and
**Ctrl+Y** undo and redo.

**Save** in the command bar writes the document's text to your project's file, with your formatting and comments
intact. The file watcher sees the write, finds that the text is already the document's, and does nothing.

## 5. A debug menu instead of an overlay

If an overlay in the way of a screen bothers you, the same editor is a control. Put an
<xref:Icy.Design.Editor.EditorWorkspace> on a page of a debug menu, with the design session from step 2:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/DesignTools.cs#workspace)]

A canvas takes one editor at a time: while the overlay is shown, the workspace waits, and the other way round.

## What you built

A running game whose pages follow your text editor, an editor over it, and F5 to see the edits that can't change the live
page, all in Debug builds only. Nothing of it is in a release.

> [!NOTE]
> Only pages loaded from markup after the session was attached can be edited. An element created in code can't be
> selected, and a style or a template edit needs the F5 reload. `NeedsReload` stays set once an edit needed it, so use
> F5 whenever the live page looks stale.

## Next

- [Markup editing and design tools](../concepts/design-tools.md): the pieces, and the editing commands in full.
- [Building a game menu](game-menu.md): a menu to try the loop on.
