# Docs Plan 3: Guides and Project — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The Guides section (an overview and seven end-to-end guides) and the Project section (five pages), in English
and Russian, built on two small library additions (theme files, source watching), ending ready for the
`v0.1.0-alpha.1` release.

**Architecture:** Two TDD library tasks first (`ThemeBuildingExtensions` in `IcyUI`, `DesignSession.WatchSourceFiles` in
`IcyUI.Design`). Then one task per guide: its markup snippets (behaviour-tested in `DocsMarkupSnippetTests`), its C#
`#region`s (Stride snippets; MonoGame snippets where the engine code differs), and the English page with a Russian stub.
Then the Project pages, Ivan's review (a stop), the Russian pages, and the release checklist.

**Tech Stack:** DocFX, IcyUI markup, C# (Stride and MonoGame snippet projects), xUnit.

**Spec:** `docs/superpowers/specs/2026-10-13-docs-plan-3-amendments-design.md` (binding), on top of
`docs/superpowers/specs/2026-10-12-docs-plans-2-3-design.md` (§2 guides/project order, §5 content standards, §6
workflow) and the non-superseded parts of `2026-10-10-docs-site-design.md`.

## Global Constraints

- Library code: StyleCop clean (Release makes warnings errors), complete XML docs, block-scoped namespaces, copyright
  header on new files, file line endings preserved. TDD: failing test first, watched failing.
- Guide shape (spec §5): one goal, end to end; numbered steps; "What you built" and "Next" at the end. About 900–1600
  words. C# in snippet projects (namespaces `MyGame.*`); both engines where the code differs (tabs
  `# [Stride](#tab/stride)` / `# [MonoGame](#tab/monogame)`, as in `getting-started/first-screen.md`).
- Markup first; every markup snippet loads and is behaviour-tested in `DocsMarkupSnippetTests`; each new snippet is
  checked once against a deliberately broken copy (spec Testing).
- Every `<xref:…>` uid is verified to exist in `docfx/api/*.yml` before use (`grep -l "^- uid: <uid>$" docfx/api/*.yml`).
- Every behavioural claim is checked against the code while writing. Shipped behaviour only; gaps go in
  `> [!NOTE]` boxes. **A library bug found at any step stops the plan; ask Ivan.**
- Display settings follow Ivan's rule: Apply and Cancel, and an applied change reverts on a timer unless confirmed.
- Russian: `docfx/ru/_glossary.md` terms (add new ones in the translation task); code stays English; `source-hash:`.
- Strict docs build (`bash .github/scripts/build-docs.sh`) exits 0 with zero DocFX warnings (the known
  NU1902/NU1903 ImageSharp audit lines from the API metadata step are environment noise, not doc warnings).
- No release action (tag, push, publish) without Ivan.

## Review Focus

1. **`ExtendDefaultTheme` must keep the built-in look for a styled type**: an extension's
   `<Style TargetType="Button" Background="Teal"/>` keeps the button template and hover/press states. Pinned in Task 1.
2. **A save from the in-game editor must not reload its own document** (the file's text equals the document's).
   Pinned in Task 2.
3. **Editors that save by rename (VS Code, Rider)** must still trigger a reload. Pinned in Task 2.
4. **The settings page's revert timer**: an applied UI size that isn't confirmed returns to the previous value. Pinned
   in Task 3's snippet test.
5. **Russian anchors** in the guides resolve (strict build + `check-navigation.sh`).

---

## Verified facts (gathered 2026-10-13)

- **Theme today:** `ThemeConfiguration.Theme` (`ResourceDictionary?`); engine defaults call `UseDefaultTheme()` (an
  `AfterBuild` step that loads the embedded `DefaultTheme.xml` when `Theme` is null); `ConfigureTheme(Action<ThemeConfiguration>)`
  is an `AfterBuild` step; `IcyConfiguration.UseDefaultTheme()` loads it on a built configuration. `Canvas` merges
  `config.Theme.Theme` into its `Resources` in its constructor, so a theme is fixed per canvas at creation.
  `ResourceDictionary.TryGetValue` searches own entries, then `MergedDictionaries` last to first; implicit styles live
  under `ResourceDictionary.GetImplicitStyleKey(type)` = `"#implicit-style:" + FullName`. `Style.BasedOn`,
  `Style.ReplacesTheme`. A page style applies on top of the theme's style unless it includes it or `ReplacesTheme`.
- **Design/hot reload:** `DesignSession.Attach(configuration)`, `UseSourceRoot`, `FindSourceRoot`, `SourcePathResolver`,
  `Documents`; documents are added in `OnDocumentCompleted`. `DesignDocument.ReloadFromSource()` reads the resolved
  file and applies it as one undoable edit (then `MarkSaved`); `Text`, `SourcePath`. `EditorOverlay.Attach(canvas,
  design)` (F4), `EditorWorkspace`. `Dispatcher.GetCurrentThreadDispatcher()`, `Invoke(action, priority)` is
  lock-protected and processed in `Canvas.Render` (`Dispatcher.Update`).
- **Menus:** `Frame` + `Frame.Navigation` (`INavigationService`: `NavigateTo(page)`, `Navigate(path)`,
  `TryNavigateBack`, `TryNavigateNext`, `CurrentPage`); `Page.KeepAlive`. `Dialog`, `Dialog<TResult>.ShowAsync(canvas)`,
  `MessageBox.ShowAsync(canvas, message, buttons)`. UI size: `Configuration.Scaling.UserScale`. Revert countdown
  pattern: `Shared Samples/ScalingDemo.cs` (10 s, an animated bar's `Completed` reverts).
- **Gamepad/focus:** `Canvas.Focus`, `FocusedElement`, `MoveFocus`, `IsKeyboardNavigationEnabled`,
  `UIElement.IsFocusable`, `IsFocusScope` (Window/Dialog are scopes; focus returns on close); keys: arrows/D-pad
  spatial, Tab/stick-click order, Enter/A activate, Escape/B close, X/Y back/forward in `Frame`. The `Focused` visual
  state (theme `FocusStates`). Key bindings aren't configurable yet (roadmap).
- **Custom controls:** derive from `Control` (template + chrome) or `UIElement`; properties with `[RegisterReference]`
  and `SetProperty` (plus `[AffectsMeasure]`/`[AffectsArrange]`/`[Category]`/`[DefaultValue]`); `MeasureContent`,
  `ArrangeContent`, `OnApplyTemplate` + `GetTemplateChild<T>("PART_X")`; `ControlState` flags drive visual states;
  property stores are discovered by reflection (no registration call). Markup reaches the type through a
  `clr-namespace:` prefix or `Types.Markup.RegisterShortName<T>()`. Example to follow: `ProgressBar` (`PART_Fill`).
- **Engine integration (FNA target; FNA's API is XNA's, the same as MonoGame's):** `IIcyHost` (`Configuration`,
  `MainCanvas`, `Canvases`); `IRenderContext` (`Begin/End/Flush`, `Draw(ITexture, in TextureRenderingOptions)`,
  `CreateTexture`, `WhiteTexture`, `Transform`, `ViewportSize`, `DisplayScale`, `ViewportResize`,
  `DisplayScaleChanged`, effects); `IInputSystem` (`Events`, `Keyboard`, `Mouse`, `Gamepad`, `Touch`, `Clipboard`,
  device lookup, `IInitializable`); `IAssetContextFactory`, `IAssetImporter<ITexture>`; `IcyConfigurationBuilder`
  sections. The MonoGame host: a `DrawableGameComponent` (`DrawOrder = int.MaxValue`) that initializes input, pumps
  `IUpdateableInput.Update` and display-scale polling in `Update`, and calls `Canvas.Render()` per canvas in `Draw`;
  `UseIcyUI` builds the configuration (`WithDefaultMonoGameConfiguration`) then the user lambda, adds the component.
  `sources/IcyUI.MonoGame` is ~2000 lines and the reference implementation; `IcyUI.FNA` holds only `Class1.cs`.
- **Localization:** as shipped (`docfx/en/concepts/localization.md`); `LocalizationDemo` in `Shared Samples`.

---

### Task 1: Theme files (`UseThemeFile`, `ExtendDefaultTheme`)

**Files:**
- Create: `sources/IcyUI/Configuration/ThemeBuildingExtensions.cs`
- Modify: `sources/IcyUI/UI/ResourceDictionary.cs` (an `internal static bool IsImplicitStyleKey(string key)`)
- Test: `sources/IcyUI.Tests/Configuration/ThemeFileTests.cs`

**Interfaces — Produces:** `ThemeBuildingExtensions.UseThemeFile(this IThemeConfigurationBuilder, string path)`,
`ThemeBuildingExtensions.ExtendDefaultTheme(this IThemeConfigurationBuilder, string path)`, both returning the builder.

- [ ] **Step 1: Failing tests.** In a temp folder (as `PoLocalizerTests` does), write theme files and build
  configurations with `new IcyConfigurationBuilder().ConfigureRendering(new FakeRenderContext()).ConfigureInput(new
  FakeInputSystem()).ConfigureTypes().ConfigureAssets().WithDefaultAssetContext(new AssetContext(root + sep))`, then
  `.UseDefaultTheme()` (as the engines do), then the call under test. Tests:
  - `UseThemeFile_ReplacesTheTheme`: file `<ResourceDictionary><Style TargetType="Button" Padding="99"/></ResourceDictionary>`;
    a `Button` added to `new Canvas(config)` gets `Padding` 99 and has no `Template` (the built-in template is gone).
  - `ExtendDefaultTheme_ImplicitStyle_KeepsTheBuiltInTemplateAndStates`: file
    `<ResourceDictionary><Style TargetType="Button" Background="#FF008080"/></ResourceDictionary>`; the attached button
    has a `Template`, its `Background` is teal, and `ControlState = Hovered|Pressed` shows the built-in pressed color
    `#FF2E2E38` (built-in states kept, last-declared tie rule).
  - `ExtendDefaultTheme_UnstyledTypes_KeepTheBuiltInStyle`: a `CheckBox` still gets the built-in template.
  - `ExtendDefaultTheme_ExplicitBasedOnAndReplacesTheme_AreLeftAlone`: a style with `ReplacesTheme="True"` has no
    `BasedOn` afterwards.
  - `ExtendDefaultTheme_KeyedResources_FromBothAreFound`: `config.Theme.Theme.TryGetValue("IcyDefaultButtonTemplate", …)`
    and a keyed brush from the file both resolve.
  - `MissingThemeFile_FailsTheBuild`: `Build()` throws, and the message names the file.
- [ ] **Step 2: Run, watch them fail** (compile error: no `UseThemeFile`).
- [ ] **Step 3: Implement.**

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.UI;
using Icy.UI.Styles;

namespace Icy.Configuration
{
    /// <summary>
    /// Builder extensions for the theme: a theme from a file, or the built-in theme extended by one.
    /// </summary>
    public static class ThemeBuildingExtensions
    {
        /// <summary>
        /// Uses the resource dictionary in the file at <paramref name="path"/> as the theme, instead of the built-in one.
        /// </summary>
        /// <param name="builder">The theme section of a configuration builder.</param>
        /// <param name="path">The theme file, relative to the configuration's assets, such as <c>Themes/Dark.xml</c>.</param>
        /// <returns>The same section, for chaining.</returns>
        /// <remarks>
        /// <para>
        /// The file is a markup <see cref="ResourceDictionary"/> loaded when the configuration is built. It replaces the
        /// built-in theme entirely: controls get only the styles and templates the file gives them. To change a few
        /// things and keep the rest, use <see cref="ExtendDefaultTheme"/>.
        /// </para>
        /// <para>A theme is chosen at startup: canvases take it when they're created.</para>
        /// </remarks>
        public static IThemeConfigurationBuilder UseThemeFile(this IThemeConfigurationBuilder builder, string path)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(path);
            builder.AfterBuild(c => c.Theme.Theme = c.Assets.Load<ResourceDictionary>(path));
            return builder;
        }

        /// <summary>
        /// Layers the resource dictionary in the file at <paramref name="path"/> over the built-in theme.
        /// </summary>
        /// <param name="builder">The theme section of a configuration builder.</param>
        /// <param name="path">The file, relative to the configuration's assets, such as <c>Themes/MyGame.xml</c>.</param>
        /// <returns>The same section, for chaining.</returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item><description>The file's resources win over the built-in theme's under the same key.</description></item>
        /// <item><description>
        /// An implicit style in the file builds on the built-in style for its type: it keeps the built-in template and
        /// visual states and changes only what it sets, as a style in a page's resources does. A style with its own
        /// <see cref="Style.BasedOn"/>, or <see cref="Style.ReplacesTheme"/> set, is left as written.
        /// </description></item>
        /// <item><description>Types the file doesn't style keep the built-in style.</description></item>
        /// </list>
        /// <para>A theme is chosen at startup: canvases take it when they're created.</para>
        /// </remarks>
        public static IThemeConfigurationBuilder ExtendDefaultTheme(this IThemeConfigurationBuilder builder, string path)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(path);
            builder.AfterBuild(c =>
            {
                ResourceDictionary builtIn = c.Theme.Theme ?? c.UseDefaultTheme().Theme.Theme!;
                ResourceDictionary extension = c.Assets.Load<ResourceDictionary>(path);
                foreach (string key in extension.Keys)
                {
                    if (ResourceDictionary.IsImplicitStyleKey(key)
                        && extension[key] is Style { BasedOn: null, ReplacesTheme: false } style
                        && builtIn.TryGetValue(key, out object? value)
                        && value is Style builtInStyle)
                    {
                        style.BasedOn = builtInStyle;
                    }
                }

                var theme = new ResourceDictionary();
                theme.MergedDictionaries.Add(builtIn);
                theme.MergedDictionaries.Add(extension);
                c.Theme.Theme = theme;
            });
            return builder;
        }
    }
}
```

  (Adapt member names to the real API: `ResourceDictionary`'s indexer/`Keys`, `IConfigurationBuilder.AfterBuild`'s
  return type, and `WithDefaultAssetContext`; check each with `grep` before writing.) Add to `ResourceDictionary`:
  `internal static bool IsImplicitStyleKey(string key) => key.StartsWith(ImplicitStyleKeyPrefix, StringComparison.Ordinal);`.
- [ ] **Step 4: Tests pass; full suite; zero warnings.** Commit "Themes: UseThemeFile and ExtendDefaultTheme".

### Task 2: Source watching (`DesignSession.WatchSourceFiles`)

**Files:**
- Modify: `sources/IcyUI.Design/DesignSession.cs` (public `WatchSourceFiles()`, an internal `DocumentTracked` event raised
  in `OnDocumentCompleted`)
- Create: `sources/IcyUI.Design/SourceFileWatcher.cs` (internal, `IDisposable`)
- Test: `sources/IcyUI.Tests/Design/SourceWatchingTests.cs`

**Interfaces — Produces:** `public IDisposable DesignSession.WatchSourceFiles()`.

- [ ] **Step 1: Failing tests** (temp folder; load a page through the session with its source path resolved to the temp
  file, as existing Design tests do — find their helper with `grep -rn "UseSourceRoot\|SourcePathResolver" sources/IcyUI.Tests/Design`).
  Pump the UI thread by calling `Dispatcher.GetCurrentThreadDispatcher().Update()` in a loop with short sleeps, up to 2 s
  (a helper `PumpUntil(Func<bool>)`); the watcher must never apply on its own thread.
  - `SavedFile_ReloadsItsDocument_OnTheUiThread` (write new text → after pumping, `document.Text` equals it and the live
    element changed);
  - `SeveralQuickWrites_ReloadOnce` (count `DesignDocument.Changed` events);
  - `RenameSave_IsSeen` (write a temp file, `File.Move(tmp, path, overwrite: true)`);
  - `TextEqualToTheDocument_DoesNotReload` (the in-game editor's own save);
  - `Dispose_StopsWatching`;
  - `DocumentTrackedLater_IsWatched`.
- [ ] **Step 2: Watch them fail.**
- [ ] **Step 3: Implement** per spec §2: one `FileSystemWatcher` per folder (`Filter = "*.xml"`, `NotifyFilter =
  LastWrite | FileName`, `Changed` + `Renamed`), a per-path debounce (150 ms; a `System.Threading.Timer` per pending
  path, or one timer and a `ConcurrentDictionary<string, DateTime>`), then `dispatcher.Invoke(() => Reload(path))`
  with the dispatcher captured in `WatchSourceFiles`. `Reload` finds documents whose resolved source path equals `path`
  (case-insensitive on Windows), skips when `File.ReadAllText(path) == document.Text`, and calls `ReloadFromSource()`.
  An `IOException` while reading retries once after 150 ms. XML docs on `WatchSourceFiles` per spec. Dispose disposes
  the watchers and timers and unsubscribes from `DocumentTracked`.
- [ ] **Step 4: Tests pass; full suite; zero warnings.** Commit "Design: WatchSourceFiles reloads pages saved in a text
  editor".

### Tasks 3–9: Guides (English) with Russian stubs

One task per guide. Each task:

1. writes its markup snippets under `docfx/snippets/markup/guides/<guide>/*.xml` with behaviour tests in
   `DocsMarkupSnippetTests` (checked once against a broken copy);
2. writes its C# in `sources/IcyUI.Docs.Snippets.Stride/Guides/<Guide>.cs` (and `…MonoGame/Guides/<Guide>.cs` where
   the code differs), regions named in the task; builds both snippet projects;
3. writes `docfx/en/guides/<guide>.md`, adds it to `docfx/en/guides/toc.yml` and `docfx/ru/guides/toc.yml` (Russian
   title), and a Russian stub (`translation: stub`);
4. runs `build-docs.sh` (exit 0), `check-translations.sh`, `check-navigation.sh`, the test suite; commits
   "Docs: guide — <title>".

Guide skeleton:

```markdown
# <Title>

<What you'll build, in two or three sentences, and what you need first (links).>

## 1. <Step>
...
## What you built
## Next
```

- [ ] **Task 3: Building a game menu** (`game-menu.md`). A main menu (title, Play, Settings, Quit) and a pause menu
  (Resume, Settings, Quit to menu) in markup with a view model; a `Frame` hosting the Settings page; Escape/B and the
  Back button. Settings page: a UI-size slider (`Scaling.UserScale`) following the Apply/Cancel/timed-revert rule
  (10 s countdown, revert unless confirmed; pattern from `ScalingDemo`), and a language choice deferred to the
  localization guide (link). Game-loop code per engine: pausing gameplay while the pause menu is open (a `paused` flag
  the game's own update checks; IcyUI keeps animating, link Animations). Snippets: `main-menu.xml`, `pause-menu.xml`,
  `settings.xml`; regions `menu-setup`, `pause-toggle` (both engines), `apply-revert`. Test: the revert test from Review
  Focus 4 (drive the view model: apply, let the countdown's animation complete by pumping a fake clock or calling the
  view model's timeout path, assert `UserScale` is back).
- [ ] **Task 4: Custom controls** (`custom-controls.md`). A `HealthBar` control: `Control` subclass with `Value`,
  `Maximum`, `FillBrush` (`[RegisterReference]`), `PART_Fill` from the template, `MeasureContent`/`ArrangeContent`,
  a `Critical` look below 25 % via a visual state driven from code (custom `ControlState` flags aren't available: use an
  existing flag such as `Highlighted`, and say so), markup use through `clr-namespace` and `RegisterShortName`, and its
  default look in the game's theme extension (`ExtendDefaultTheme`, link Task 5's guide). The control's code lives in the
  Stride snippet project (`MyGame.Controls.HealthBar`) so the markup snippet can load it in tests (reference the snippet
  assembly from the tests project only if it already is; otherwise put the control in `IcyUI.Tests/Docs/` like
  `DocsSnippetViewModel` and show the same code from a region — decide by checking the test project's references, ledger
  the ruling).
- [ ] **Task 5: Custom themes** (`custom-themes.md`). Extending the built-in theme with a game palette
  (`ExtendDefaultTheme("Themes/MyGame.xml")`): implicit styles that recolor buttons and lists while keeping their
  templates and states, keyed resources, a custom `ControlTemplate` for one control family, `ReplacesTheme`; a whole
  theme with `UseThemeFile` (start from a copy of `DefaultTheme.xml`); themes are chosen at startup (NOTE box). Snippets:
  `theme-extension.xml` (loaded by a test through `ExtendDefaultTheme` from a temp copy), region `theme-setup`.
- [ ] **Task 6: Hot reload and the in-game editor** (`hot-reload-and-editor.md`). Debug-only package reference;
  `DesignSession.Attach` + `UseSourceRoot(FindSourceRoot(...))` + `WatchSourceFiles()` + `EditorOverlay.Attach`; the
  edit loop from a text editor and from the overlay; saving; what needs a reload (styles, templates); `EditorWorkspace`
  for a debug menu. Regions `design-setup` (Stride and MonoGame). Links the Design tools concept page.
- [ ] **Task 7: Gamepad-friendly UI** (`gamepad.md`). Focus on open (`Canvas.Focus` on the first button), spatial
  navigation layouts that work (rows, grids), focus scopes for dialogs, a clear focus look (theme `FocusStates`), lists
  and scrolling with the D-pad, turning navigation off during gameplay (`IsKeyboardNavigationEnabled`), and the key
  table; NOTE: keys aren't rebindable yet (roadmap). Snippet `gamepad-menu.xml` with a test that `MoveFocus`/spatial
  moves reach every button.
- [ ] **Task 8: Localizing your game UI** (`localization.md`). End to end: keys in markup (`{Localize}`), `en.po` and
  `ru.po` (Poedit, how translators get the files), `UsePoLocalization`, a language setting in the Task 3 settings page
  (sets `Configuration.Culture`; texts and number formats switch), code-built text (`Localize`), a mod layer
  (`CompositeLocalizer`, no default), fonts for the languages (NOTE). Reuse `LocalizationDemo`'s approach; snippet
  `localized-menu.xml` + test with a `DictionaryLocalizer`; regions `po-setup`, `language-setting`.
- [ ] **Task 9: Writing an engine integration** (`engine-integration.md`). What an integration provides, in order:
  an `IRenderContext` (sprite-batch drawing, textures, viewport and display scale), an `IInputSystem` (devices, text
  input, clipboard), asset contexts and a texture importer, the host (an `IIcyHost` game component: initialize input,
  pump input and display scale in Update, `Canvas.Render()` per canvas in Draw, `DrawOrder` last), and a `UseIcyUI`
  extension that builds the configuration then applies the user's lambda. FNA as the target (XNA API, so the skeleton
  compiles against MonoGame in `IcyUI.Docs.Snippets.MonoGame/Guides/EngineIntegration.cs`; NOTE: `IcyUI.FNA` is a stub,
  and FNA-specific details such as native libraries are out of scope); link the MonoGame integration source on GitHub
  as the complete reference. Regions `host`, `render-context-outline`, `input-outline`, `use-icyui`. No markup snippet.

### Task 10: Guides overview and the Project pages (English) with Russian stubs

- [ ] `docfx/en/guides/index.md` (replace the stub: one paragraph per guide, in order) and `guides/toc.yml` order:
  Overview, Building a game menu, Custom controls, Custom themes, Hot reload and the in-game editor, Gamepad-friendly UI,
  Localizing your game UI, Writing an engine integration.
- [ ] `docfx/en/project/`: `index.md` (overview), `status-and-roadmap.md` (alpha status; the roadmap list from the
  amendment spec, verbatim in meaning, "planned", no dates), `known-issues.md` (`CLAUDE.md`'s Known issues in user
  terms: DesktopGL on Windows on ARM64 can hang on exit; no touch on DesktopGL; UI scaling needs a PerMonitorV2
  manifest; plus the deferred limitations users can hit: themes chosen at startup, `{Localize}` in style setters, keys
  not rebindable), `changelog.md` (alpha.1 by area: controls, markup, binding (typed bindings, culture), input, hosts and
  configuration, localization, editor, docs), `building-from-source.md` (`CLAUDE.md`'s build/test/docs commands for
  outsiders: SDK pin, solution, tests, docs build and preview, StyleCop rule, PR expectations), `history-and-license.md`
  (MysticUI → AquaUI → IcyUI, MIT). `project/toc.yml` in that order; Russian toc titles; stubs.
- [ ] Build, translations, navigation; commit "Docs: guides overview and project pages".

### Task 11: Ivan reviews the English pages (stop)

Serve the site (`dotnet docfx serve docfx/_site`), give Ivan the URLs of the guides and project pages, wait for approval
or edits; apply edits, rebuild, commit.

### Task 12: Russian pages

Translate every approved Guides and Project page (and both overviews), add glossary terms, set `source-hash`. Checks:
build exit 0, `check-translations.sh` reports every page fresh with no stubs, navigation passes. Commit "Docs (ru):
guides and project".

### Task 13: Verification, whole-branch review, release checklist

- [ ] Release build (shipped libraries warning-free), tests `Failed: 0`, docs build, navigation, translations (no stubs),
  no controls snippet newer than its screenshot.
- [ ] Final whole-branch review by a fresh reviewer; fix pass.
- [ ] Release checklist for Ivan (no action taken): `<Version>` in `sources/Directory.Build.targets` is
  `0.1.0-alpha.1`; CI green on both runners; the steps in `CLAUDE.md` § Release (tag `v0.1.0-alpha.1`, approve the
  `nuget` deployment after checking the `packages` artifact, approve `github-pages` after the packages are published);
  the one-time `github-pages` environment setup still pending in `CLAUDE.md` § Documentation. Report to Ivan and stop.
