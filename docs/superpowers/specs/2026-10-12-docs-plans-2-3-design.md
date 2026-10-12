# Documentation Plans 2a, 2b and 3: navigation, controls, concepts, guides, project

> Design spec, discussed with Ivan on 2026-10-12. Rescopes Plans 2 and 3 of
> `2026-10-10-docs-site-design.md`, which stays in force except where this spec says it's superseded.

## Context

- **Plan 1 is done.** The site has Home, Installation, Getting started (Stride, MonoGame), Your first screen, the
  markup reference and the API reference, in English and Russian (7 pages each).
- **Since the 2026-10-10 spec,** the host API (`IIcyHost`), the fluent configuration, logging, the font section,
  text-as-content and Button's centered default have landed. The Concepts list is updated for them.
- **The Docs workflow fails on every push to `platform-independent`:** the `github-pages` environment only accepts
  deploys from the default branch (`main`).
- **Nothing is deployed.** `https://icyui.dev` answers GitHub's "Site not found".
- **Local preview looks broken when a page is opened from disk.** The modern DocFX template loads `toc.json` with
  `fetch` and runs search in a web worker; a `file://` page shows no navbar, no sidebar and no working search. Over
  HTTP (`dotnet docfx serve docfx/_site`) the navbar, sidebar and language link render (checked with headless Edge on
  2026-10-12).
- **Library localization** still ships in alpha.2, so "Localizing your game UI" stays out.

## Decisions

| Question | Decision |
|---|---|
| Release gate | `v0.1.0-alpha.1` is tagged only after Plans 2a, 2b and 3 are done in both languages. |
| Deployment | The site deploys only when a `v*` tag is pushed, from the tagged commit. Pushes and PRs only build. |
| Work split | Three plans: **2a** CI, navigation, capture tool, controls; **2b** Concepts; **3** Guides and Project. Each ends with a green docs build, not a deploy. |
| Concepts | 11 pages (list in Design §2). |
| Controls catalog | An index plus 12 **family** pages, not one page per control. |
| Screenshots | Captured from the pages' own markup snippets by a separate tool project, `IcyUI.Docs.Capture`. |
| Guides | All six from the 2026-10-10 spec. |
| Translation | Per plan: English pages first (with Russian stubs), Ivan reviews the English, then one task translates the approved pages. |
| Library bugs found while writing | The plan stops and asks Ivan. Nothing is fixed or worked around without his decision. |

## Design

### 1. Deployment and docs CI (Plan 2a)

- **`.github/workflows/docs.yml`:**
  - triggers on pushes to `platform-independent`, on pull requests, and on pushed `v*` tags;
  - the build job (snippet projects, `build-docs.sh`, `check-translations.sh`) runs on all of them;
  - the upload and the deploy job run only for a `v*` tag (`startsWith(github.ref, 'refs/tags/v')`).
- **Ivan's one-time setup:** in **Settings → Environments → github-pages**, add the deployment tag rule `v*` (as for
  `nuget`). Recorded in `CLAUDE.md`'s one-time hosting setup.
- **`CLAUDE.md`'s release steps** gain: "the docs site deploys with the tag; check the Docs run as well as the Release
  run". The Documentation section's "deploys on pushes to `platform-independent`" is corrected.
- **Local preview:** `build-docs.sh` ends by printing `Preview: dotnet docfx serve docfx/_site`. `CLAUDE.md` says a page
  opened from disk shows no navbar, sidebar or search.

### 2. Information architecture and navigation (Plan 2a, first content task)

**Navbar** (root `toc.yml`, both languages): Getting started · Concepts · Controls · Guides · Markup · API · Project.
Concepts, Guides and Project get a stub `index.md` in Plan 2a ("being written"), so the navbar never links to a
missing page; their plans replace the stubs.

**Sidebar:** each section folder has its own `toc.yml` listing its pages in reading order. The modern template shows it
on the left and highlights the current page.

| Section | Sidebar order | Plan |
|---|---|---|
| `concepts/` | Overview, then: 1. Canvas, the host and the element tree (`IIcyHost`, `MainCanvas`/`Canvases`, overlays, root elements); 2. Configuration (the builder, sections, engine defaults, deferred steps, user overrides); 3. Layout (measure/arrange, alignment including Button's centered default, NaN-as-unset limits, panels: StackPanel, Grid, WrapGrid, ScrollViewer, SplitPane); 4. Data binding (modes, converters, text conversion, validation errors, missing-path warnings); 5. Resources, styles and themes (value precedence: local, animation, visual state, style); 6. Control templates and visual states; 7. Input, focus and spatial navigation (keyboard, gamepad, touch gestures, drag and drop, per-engine gaps); 8. Animations; 9. DPI and UI scaling; 10. Assets and fonts (system fonts, `ImportFont`, the fallback font, font licensing); 11. Logging and diagnostics (the logging section, providers, the F1/F2 debug tools) | 2b |
| `controls/` | Overview (thumbnail grid), then the families in §4's order | 2a |
| `guides/` | Overview, then: Building a game menu; Custom controls; Custom themes; Hot reload and the in-game editor; Gamepad-friendly UI; Writing an engine integration | 3 |
| `project/` | Status and roadmap; Known issues; Changelog; Building from source and contributing; History and license | 3 |

- **Home page:** one link card per section (Getting started, Concepts, Controls, Guides, Markup, API), replacing the
  current link list.
- **Russian:** the same files under `docfx/ru/` with translated titles; stubs keep `/ru/` free of 404s until the
  plan's translation task.

### 3. Capture tool: `sources/IcyUI.Docs.Capture` (Plan 2a)

- **Project:** a code-only Stride app (`OutputType` `WinExe`, `net10.0-windows`, `IsPackable` false), set up like
  `Stride Sample` (same package references and code-built graphics compositor). It's in `IcyUI.sln`, so CI compiles it.
  No StyleCop (it isn't shipped).
- **Input:** every `*.xml` under `docfx/snippets/markup/controls/`. The repository root is found by walking up from the
  executable to the folder that contains `.git`.
- **Output:** `docfx/images/controls/<name>.png` for `<name>.xml`.
- **Per snippet:**
  1. Load the markup with the engine's default configuration (default theme, system fonts, Segoe UI) and set a
     `DocsSnippetViewModel` as its data context, the same class `DocsMarkupSnippetTests` uses. The class moves to a
     place both projects can compile (a linked source file).
  2. Put the root on a canvas at scale 1.0, whatever the machine's DPI, laid out at its desired size.
  3. Render one frame on the theme's backdrop color.
  4. Read the back buffer, crop it to the root's bounds plus 16 px padding, and save a PNG.
- **Run:** `dotnet run --project sources/IcyUI.Docs.Capture`. It writes all images and exits with code 0, or non-zero
  with the failing snippet's name when one fails to load or render.
- **Freshness check:** `build-docs.sh` warns (doesn't fail) for each controls snippet with no PNG or with a PNG older
  than the snippet. CI can't run the capture (no GPU or window on the runner); Ivan runs it.
- **Release builds of the shipped libraries are unaffected.** No library code changes for the tool.

### 4. Controls catalog (Plan 2a)

One page per family:

| Page | Members |
|---|---|
| `buttons.md` | Button, ToggleButton, CheckBox |
| `text.md` | TextBlock, TextBox |
| `images.md` | Image, Icon |
| `range.md` | Slider, ProgressBar |
| `lists.md` | ItemsControl, ListBox, ComboBox, Dropdown, WrapGrid |
| `tree-view.md` | TreeView |
| `navigation.md` | TabControl, Frame, Page |
| `expander-split.md` | Expander, SplitPane |
| `scrolling.md` | ScrollViewer |
| `panels.md` | StackPanel, Grid |
| `dialogs.md` | Dialog, MessageBox, Window |
| `editors.md` | ColorPicker, ColorPickerButton, PropertyGrid |

- **Page order,** once for the family (the 2026-10-10 spec's control-page order):
  1. screenshot;
  2. purpose and when to use it;
  3. minimal markup;
  4. key properties, linked to the API;
  5. events and commands;
  6. styling hooks (template parts, visual states);
  7. keyboard, gamepad and touch behaviour;
  8. see also.

  Then a short subsection per member for what differs from the family.
- **Every family has at least one captured snippet** in `docfx/snippets/markup/controls/`.
- **`controls/index.md`:** a grid of the family screenshots, each linking to its page.

### 5. Content standards (additions to the 2026-10-10 spec §7)

- **Concept page:** the idea; a minimal example (a tested markup snippet or compiled C#); how it interacts with
  neighbouring concepts; related links. Member lists stay in the API reference, linked with `<xref:…>`.
- **Guide:** one goal, end to end, ending with "what you built" and "next". C# in the snippet projects, for both
  engines where they differ (for example, the pause menu's game-loop code).
- **Building a game menu** follows Ivan's display-settings rule: settings that change the display have Apply and
  Cancel, and an applied change reverts on a timer unless confirmed.
- **Writing an engine integration** uses the `IIcyHost` contract, `IRenderContext` and `IInputSystem`, with FNA as the
  example target, and says that `IcyUI.FNA` is a stub today.
- **Project pages:**
  - *Status and roadmap:* alpha status; alpha.2 is library localization; nothing undecided is promised.
  - *Known issues:* `CLAUDE.md`'s Known issues in user terms (DesktopGL on Windows on ARM64 can hang on exit; no touch
    input on DesktopGL; UI scaling needs a PerMonitorV2 manifest).
  - *Changelog:* alpha.1 summarized by area (controls, markup, binding, input, hosts and configuration, editor, docs).
  - *Building from source and contributing:* `CLAUDE.md`'s build, test and docs commands, written for outsiders.
  - *History and license:* the MysticUI → AquaUI → IcyUI history and the MIT license.
- **Honesty:** shipped behaviour only; engine gaps in `> [!NOTE]` boxes.
- **Glossary:** new Russian terms go into `docfx/ru/_glossary.md` in the translation task that first needs them.

### 6. Workflow inside each plan

1. English pages, each with a Russian stub (`translation: stub`).
2. **Ivan reviews the English pages.** The plan waits for his approval or edits.
3. One translation task: every approved page in Russian, with `source-hash`, replacing its stub.
4. The plan's verification (Testing below). No deploy.

A library bug found at any step stops the plan; Ivan decides what happens to it.

## Plans

- **Plan 2a:**
  1. docs CI (deploy on tags) and the `CLAUDE.md` updates;
  2. navigation (navbar, section `toc.yml`s, stub section pages, Home cards);
  3. the capture tool;
  4. the controls snippets and the 13 controls pages (English);
  5. Ivan's review, then the Russian pages.
- **Plan 2b:** the 11 Concepts pages and the Concepts overview (English), Ivan's review, the Russian pages.
- **Plan 3:** the 6 Guides and the Guides overview, the 5 Project pages (English), Ivan's review, the Russian pages.
  Then the release checklist: `CLAUDE.md`'s release steps, with the docs deploying on the tag.

## Testing

Each plan ends with:

- **Strict builds:** both DocFX builds pass with zero warnings (`build-docs.sh`).
- **Snippets:** every included C# snippet compiles in the solution build; every markup snippet loads in
  `DocsMarkupSnippetTests`. Each new markup snippet is checked once against a deliberately broken copy.
- **Navigation check:** with `docfx serve` running, headless Edge renders three pages per section (English and
  Russian), and the rendered DOM has the navbar links, the section's sidebar entries and the EN/RU link. A local
  script; not in CI.
- **Translations:** `check-translations.sh` reports every page fresh, with no stubs, stale pages or orphans.
- **Screenshots:** no controls snippet is newer than its PNG (the `build-docs.sh` warning is empty).
- **Plan 2a only:** `docs.yml` passes `actionlint`; the capture tool writes one PNG per controls snippet on Ivan's
  machine. A broken snippet makes it exit non-zero.
- **Not claimed:** Ivan's reading of every page, his Russian review, the first deploy and `https://icyui.dev`
  answering after the tag.

## Performance

- **Libraries:** no runtime cost. The capture tool is a separate, unpacked project; no library code changes for docs.
- **CI:** the capture project adds seconds to the solution build. The docs job builds on every push and PR, as now,
  and deploys only on tags.
- **Site:** static pages with a client-side search index per language, as in Plan 1.

## Known limitations

- Screenshots are captured on Windows with Segoe UI; the controls look the same elsewhere only with the same font.
- CI doesn't run the capture tool, so it can only warn about stale images.
- The navigation check runs locally (it needs a browser), not in CI.
- DocFX's English stemmer makes Russian search match exact word forms only (unchanged from Plan 1).

## Out of scope

- Library localization and its guide (alpha.2).
- MonoGame screenshot capture.
- Per-control pages beyond the families; versioned docs; analytics; PDF output.
- Any library change. Bugs found while writing go to Ivan.
