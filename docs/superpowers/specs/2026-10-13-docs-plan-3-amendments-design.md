# Docs Plan 3 amendments: localization guide, theme files, source watching, roadmap

> Design spec, discussed with Ivan on 2026-10-13. Amends `2026-10-12-docs-plans-2-3-design.md` (Plan 3) and states the
> two small library changes the guides are built on. Everything else in that spec stands.

## Context

- **Localization shipped in alpha.1** (`docs/superpowers/specs/2026-10-12-localization-design.md`), but the docs spec
  assumed alpha.2: Plan 3 had no localization guide, and the roadmap page was to say "alpha.2 is library localization".
- **Writing the Custom themes guide hit a gap.** `ConfigureTheme`'s lambda gets a `ThemeConfiguration`, which can't
  load a file; a theme file needs `builder.AfterBuild(c => c.Theme.Theme = c.Assets.Load<ResourceDictionary>(...))`.
  An implicit style in a custom theme also replaces the built-in style for that type entirely (template included),
  because the theme's dictionary lookup returns the first match.
- **Writing the Hot reload guide hit a gap.** `DesignDocument.ReloadFromSource()` exists, but nothing watches the files:
  an edit saved in a text editor applies only when game code calls it.
- **Plan 2b is done** (Concepts in both languages, 2026-10-13).

## Decisions

| Question | Decision |
|---|---|
| Localization guide | Added: **Localizing your game UI**, a 7th guide. |
| Theme files | Small library change before the guide: `UseThemeFile(path)` and `ExtendDefaultTheme(path)`. |
| Runtime theme switching | Out of scope: themes are chosen at startup, and the guide says so. |
| Source watching | Small library change before the guide: opt-in `DesignSession.WatchSourceFiles()`. |
| Roadmap page | Lists only the decided items below, as "planned", with no dates or version numbers. |

### Roadmap items (decided)

- An FNA integration (`IcyUI.FNA` is a stub today).
- Localization arguments and plural forms (`"{0} coins"`).
- Source generators: compiled `x:DataType` binding paths for AOT, and page classes generated from markup for games that
  don't load UI dynamically or prefer AOT.
- Rebindable keyboard and gamepad keys for focus navigation.
- Memory and performance work: less allocation and RAM use (spans, pooling).
- An SDK with an editor pack: a UI project structure designed separately from the game and added to it as a package,
  a directory or another source.

## Design

### 1. Theme files (`IcyUI`, `Configuration`)

Two extensions on `IThemeConfigurationBuilder`, both build steps (`AfterBuild`), so they run after the engine's
`UseDefaultTheme` step:

```csharp
/// The theme is the resource dictionary in the file at path (loaded through the configuration's assets),
/// instead of the built-in one.
public static IThemeConfigurationBuilder UseThemeFile(this IThemeConfigurationBuilder builder, string path);

/// The built-in theme, with the resource dictionary in the file at path layered over it.
public static IThemeConfigurationBuilder ExtendDefaultTheme(this IThemeConfigurationBuilder builder, string path);
```

- `UseThemeFile`: `c.Theme.Theme = c.Assets.Load<ResourceDictionary>(path)`. The engine's default theme step runs
  before it and loads the built-in theme, which is then discarded: a few milliseconds at startup, accepted (no
  coupling between the two steps).
- `ExtendDefaultTheme`: the theme becomes a new `ResourceDictionary` that merges the built-in theme, then the file.
  Merged dictionaries are searched last to first, so the file's entries win.
- **Implicit styles in an extension build on the built-in ones.** For each implicit `Style` in the file (one stored
  under an implicit-style key) whose `BasedOn` is unset and whose `ReplacesTheme` is `false`, and for whose type the
  built-in theme has a style, `BasedOn` is set to the built-in style. So `<Style TargetType="Button" Background="Teal"/>`
  in an extension keeps the button template and states and changes the background, the same rule as a style in a
  page's resources over the theme. `ReplacesTheme="True"` opts out.
- Errors: a missing file or bad markup throws from `Build()`, naming the file (`MarkupException` / the asset error), as
  `LoadMarkup` does.

### 2. Source watching (`IcyUI.Design`, `DesignSession`)

```csharp
/// Reloads a tracked page when its source file changes on disk, so edits saved in a text editor show in the running
/// game. Dispose the result to stop.
public IDisposable WatchSourceFiles();
```

- Watches the folders of the resolved source files (`SourcePathResolver`, `UseSourceRoot`) of the documents tracked
  now and of those tracked later, with one `FileSystemWatcher` per folder (`*.xml`, `LastWrite` and `Renamed`, since
  editors save by renaming a temp file).
- A change is debounced (150 ms per file, as editors write in several steps), then posted to the dispatcher of the
  thread that called `WatchSourceFiles` (the UI thread), which runs it during the next `Canvas.Render`. There it calls
  `ReloadFromSource()` on every document with that source path, unless the document's text already equals the file's
  (the save came from the editor in the game).
- A file that can't be read (still locked) is retried once after another 150 ms, then skipped until its next change.
- Opt-in and dev-only: nothing runs unless called; the package itself is referenced in Debug builds only.

### 3. Guides (Plan 3)

Seven guides, each with one goal end to end, ending with "what you built" and "next", C# in the snippet projects for both
engines where they differ:

1. Building a game menu: a main menu and a pause menu, with a settings page following the Apply/Cancel/timed-revert rule
   for display settings (UI size).
2. Custom controls.
3. Custom themes: `ExtendDefaultTheme`, `UseThemeFile`, the implicit-style rule, startup-only.
4. Hot reload and the in-game editor: `WatchSourceFiles` plus the editor overlay.
5. Gamepad-friendly UI.
6. Localizing your game UI: `.po` files end to end and a language setting.
7. Writing an engine integration: FNA as the target, `IcyUI.FNA` is a stub.

## Testing

- Theme files: `UseThemeFile` replaces the theme; `ExtendDefaultTheme` keeps built-in styles for types the file doesn't
  style, layers an implicit style over the built-in one (template kept, its own values win), leaves an explicit
  `BasedOn` and `ReplacesTheme="True"` alone, and keeps keyed resources of both; a missing file fails the build.
- Source watching: a saved file reloads its document on the next dispatcher update (not on the watcher thread); several
  writes within the debounce reload once; a rename-save is seen; identical text doesn't reload; dispose stops it; a
  document tracked after the call is watched. Tests use a temp folder and pump the dispatcher.

## Performance

- Theme files: build-time only; `UseThemeFile` costs one discarded load of the built-in theme at startup.
- Source watching: opt-in, Debug builds; one watcher per source folder, work only when a file changes.
- Guides: no runtime cost.

## Out of scope

- Runtime theme switching.
- Watching files that aren't tracked documents (resource dictionaries merged by `Source`).
