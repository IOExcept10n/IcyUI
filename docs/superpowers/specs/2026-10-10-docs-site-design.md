# Documentation site at icyui.dev (English and Russian)

> Design spec, discussed with Ivan on 2026-10-10. Last item of the release-prep list (StyleCop, review, packaging and
> CI are done). One spec for the whole site, implemented through three plans, each ending in a deployed site. Plan 1 is
> the `v0.1.0-alpha.1` milestone.

## Context

- **The `docfx/` folder is a template leftover:**
  - `index.md` is DocFX's placeholder text, and `introduction.md` and `getting-started.md` are empty.
  - `docfx.json` generates API pages from **every** `.csproj` (tests and samples included) and still has `pdf: true`.
  - DocFX isn't installed as a tool anywhere.
- **Real material exists:**
  - `markup-spec.md`: 424 lines, accurate, but written as a spec with "M0–M3" milestone notes;
  - two markup XSDs;
  - complete XML docs on all public types (~238 public classes), enforced by the zero-warning build;
  - 21 demo pages in `Shared Samples`.
- **The root `README.md` is stale.** It says "`IcyUI.MonoGame`: Primary integration (DesktopGL)" and "pre-MVP".
- **The first reader is Ivan,** building his Stride graduation game. He hasn't decided between a Game Studio project
  and a code-only game, so both must be documented and work.
- **Localization:** `ILocalizer`/`ResourceLocalizer`/`WithLocalizer` exist in core but nothing uses them. The library's
  built-in strings are English. Library localization is a separate sub-project after Plan 1 (it ships in alpha.2).
  This spec covers only localized documentation.

## Decisions

| Question | Decision |
|---|---|
| Work split | One spec; three plans, each ending in a deployed site. |
| Hosting | GitHub Pages, deployed by CI, on Ivan's domain **`https://icyui.dev`** (apex), bought at Cloudflare Registrar. |
| Versions | A single "latest" site tracking `platform-independent`; no per-release docs. |
| Languages | English (source, at `/`) and Russian (at `/ru/`, same page paths). Ivan reviews the Russian. |
| API reference | Generated from the four shipped libraries only; English only. Russian pages link to it. |
| Markup reference | At `/markup`, so the markup namespace URI `https://icyui.dev/markup` resolves to its documentation. |
| Code examples | From compiled snippet projects (C#) and loader-tested files (markup), included by DocFX. No hand-pasted multi-line code. |
| Screenshots | Captured from the sample app with a debug-only F9 helper (Stride first). |
| Stride getting started | Leads with a Game Studio project (a startup script), with a code-only section. The Game Studio path is verified by Ivan in Plan 1. |
| Translations | Each plan translates the pages it adds. CI **warns** (doesn't fail) on stale translations. Untranslated pages get a Russian stub. |

## Design

### 1. Information architecture

| Section | Pages | Plan |
|---|---|---|
| **Home** (`/`) | What IcyUI is, a hero screenshot, install commands, links into each section. | 1 |
| **Getting started** | **Installation:** packages, requirements (.NET 10, Windows), the MonoGame backend note. **Stride:** a Game Studio project via a startup script, plus a "without Game Studio" (code-only) section. **MonoGame:** `Initialize`/`Draw`, WindowsDX or DesktopGL. **Your first screen:** a small main menu in markup with a binding and a command, for both engines. | 1 |
| **Reference** | **API:** generated. **Markup reference** at `/markup`: `markup-spec.md` rewritten in reference tone (no milestone notes), with the XSDs downloadable next to it. | 1 |
| **Concepts** | Canvas and the element tree; layout (measure/arrange, alignment, NaN-as-unset limits, panels); data binding (modes, converters, validation errors); resources, styles and themes; control templates and visual states; input, focus and spatial navigation (keyboard, gamepad, touch, drag and drop); animations; DPI and UI scaling; assets and fonts. | 2 |
| **Controls** | An index page with a thumbnail grid, and one page per control. The list is derived from the code at plan time (~25 pages). | 2 |
| **Guides** | Building a game menu (a pause menu, and settings with Apply/Cancel); custom controls; custom themes; hot reload and the in-game editor (`IcyUI.Design`); gamepad-friendly UI; writing an engine integration. "Localizing your game UI" is added once library localization has shipped. | 3 |
| **Project** | Status and roadmap, known issues, changelog, building from source and contributing, history and license. | 3 |

- **Paths:** the English and Russian pages share relative paths. For example, `/getting-started/stride.html` and
  `/ru/getting-started/stride.html`.
- **Language switcher:** in the top bar, an EN/RU link that maps the current page to its counterpart.
- **Untranslated pages:** each has a Russian stub saying the page isn't translated yet, linking to the English page.
  `/ru/` navigation never 404s.

### 2. Repository layout

```
.config/dotnet-tools.json        docfx 2.81.0 (repo-local tool)
docfx/
  docfx.en.json                  metadata (four shipped .csproj) + English build -> _site/
  docfx.ru.json                  Russian build -> _site/ru/ (xref to the English xrefmap)
  en/                            English pages + toc.yml, index.md, CNAME (icyui.dev)
    markup/                      the markup reference + the two XSDs
  ru/                            Russian pages + toc.yml; _glossary.md (excluded from the build)
  images/                        shared images (controls/ for catalog screenshots)
  template/public/main.js        modern-template override: the EN/RU switcher
  snippets/
    Snippets.Stride/             core + Stride C# snippets (non-packable, in the solution)
    Snippets.MonoGame/           MonoGame C# snippets (DesktopGL compile-only, like IcyUI.MonoGame)
    markup/                      markup snippets (*.xml)
```

- **The current files move into this structure:** `docfx/docs/markup-spec.md` → `en/markup/index.md` (rewritten), and the XSDs
  → `en/markup/`. The template placeholders are deleted.
- **Why two snippet projects:** one project can't safely reference both engines, because both ship native SDL2. Core
  snippets live in `Snippets.Stride`.
- **`_site/` and `api/`** (generated metadata) are git-ignored.

### 3. Builds

- **API metadata:** `docfx metadata docfx/docfx.en.json`. It covers `IcyUI`, `IcyUI.Design`, `IcyUI.MonoGame` and
  `IcyUI.Stride`, into `docfx/api/`.
- **English:** `docfx build docfx/docfx.en.json --warningsAsErrors`.
  - It covers `en/**`, the API pages, `images/**` and `CNAME`, plus the `modern` template with the `template/`
    override. It produces `xrefmap.yml`.
  - Global metadata: `_appTitle: IcyUI`, `_enableSearch: true`, no PDF.
- **Russian:** `docfx build docfx/docfx.ru.json --warningsAsErrors`.
  - It covers `ru/**` (and `images/**`), into `_site/ru/`.
  - Its `xref` setting points at the English build's `xrefmap.yml`, with the base URL `/`, so `<xref:Icy.UI.Canvas>`
    on a Russian page links to the English API page.
- **Strictness:** `--warningsAsErrors` makes broken internal links, missing xrefs and missing snippet files or regions
  fail the build.
- **Local preview:** `dotnet docfx serve docfx/_site` after both builds.

### 4. Snippets

- **C#:**
  - Snippets live in `#region` blocks inside the snippet projects, and pages include them with
    `[!code-csharp[](../snippets/Snippets.Stride/GettingStarted.cs#region-name)]`.
  - Both projects are in `sources/IcyUI.sln` and set `IsPackable=false` (they aren't shipped libraries, so
    `Directory.Build.targets` already does this). CI builds them with the solution.
  - Snippet code is minimal: no demo-shell scaffolding. It compiles; it isn't run.
- **Markup:**
  - `docfx/snippets/markup/**/*.xml`, included with `[!code-xml[](…)]`.
  - One parameterized test in `IcyUI.Tests` (`DocsMarkupSnippetTests`) loads every file through the real markup
    loader, with the default theme and the test suite's fake rendering. Load errors fail the test, so renamed controls
    and properties are caught.
  - Snippets that need a data context declare it in a comment header naming a type in the test project, which the test
    supplies.
- **Inline code:** shell commands and one-liners may be inline. Anything longer comes from a snippet file.

### 5. Deployment: `.github/workflows/docs.yml`

- **Triggers:**
  - `push` to `platform-independent` when `docfx/**`, `sources/IcyUI*/**`, `.config/dotnet-tools.json` or the workflow
    changes: build and deploy;
  - `pull_request` with the same paths: build only.
- **Job `build`** (`windows-latest`; `contents: read`; `shell: bash`):
  1. Checkout, `setup-dotnet` from `global.json`, and the NuGet cache (same key scheme as CI).
  2. `dotnet tool restore`, then `dotnet build` of the two snippet projects.
  3. `docfx metadata`, then the English build, then the Russian build (both strict).
  4. `bash .github/scripts/check-translations.sh` (warnings only).
  5. `actions/upload-pages-artifact` with `docfx/_site`.
- **Job `deploy`:**
  - Needs `build`, and runs only on `push`.
  - `environment: github-pages`, with `permissions: pages: write, id-token: write`.
  - Runs `actions/deploy-pages`.
- **Concurrency:** group `docs-pages`, without cancelling an in-progress deploy.
- **Actions** are pinned to full SHAs, as in the other workflows:
  - `actions/upload-pages-artifact` v5.0.0, `actions/deploy-pages` v5.0.1;
  - `actions/configure-pages` v6.0.0, if needed.
  - Resolve the SHAs at plan time. Dependabot already covers `github-actions`.
- **Custom domain:** the `CNAME` file in `docfx/en/` is copied to the site root.

### 6. Translations

- **Source pointer:** every Russian page with real content has front matter
  `source-hash: <first 12 hex of SHA-256 of the English source file>`.
- **Stubs:** a stub has `translation: stub` and no hash.
- **`.github/scripts/check-translations.sh`:**
  - For every `docfx/ru/**/*.md` except `_glossary.md`, it finds the English file with the same relative path, hashes
    it, and compares the hash with `source-hash`.
  - It prints `::warning file=<ru page>::` for stale pages and for Russian pages without an English counterpart, and
    exits 0.
  - Stubs are listed in a summary, not warned about.
- **Updating a translation:** edit the Russian text, then set `source-hash` to the English file's current hash. The
  script prints the current hash for each stale page, to copy.
- **Glossary** (`docfx/ru/_glossary.md`, excluded from the build). Code identifiers, XML and API/type names stay in
  English; prose is translated. Proposed terms, confirmed by Ivan at the first review:

  | English | Russian |
  |---|---|
  | control | элемент управления |
  | binding | привязка (данных) |
  | template | шаблон |
  | style | стиль |
  | markup | разметка |
  | layout | компоновка (not разметка, which is taken by markup) |
  | focus | фокус |
  | view model | модель представления |
  | Canvas | Canvas (unchanged) |

### 7. Content standards

- **Audience and tone:**
  - The reader is a C# game developer who knows their engine but not necessarily WPF.
  - Second person, present tense, short paragraphs.
- **Members:** the XML docs stay the single source of truth. Conceptual pages link with `<xref:…>` instead of
  repeating member lists.
- **Page types:**
  - **Tutorial:** numbered, runnable steps, ending with "what you built" and "next".
  - **Concept:** the idea, then a minimal example, then related concepts.
  - **Control page,** in a fixed order:
    1. screenshot;
    2. purpose and when to use it;
    3. minimal markup;
    4. key properties table, linked to the API;
    5. events and commands;
    6. styling hooks (template parts, visual states);
    7. keyboard, gamepad and touch behaviour;
    8. see also.
  - **Guide:** one goal, start to finish.
- **Honesty:**
  - Document shipped behaviour only.
  - Known gaps are called out in `> [!NOTE]` boxes, for example "touch input isn't available on MonoGame DesktopGL".
  - Planned features are never described as present.

### 8. Screenshot helper (Plan 2)

- **Trigger:** in `Shared Samples/SampleShell.cs`, F9 (registered only under `#if DEBUG`) calls a host-provided
  callback with the current demo's content element and the demo name.
- **Stride capture:** `Stride Sample` implements the callback. It reads `GraphicsDevice.Presenter.BackBuffer` after the
  frame, crops to the content element's bounds (converted to back-buffer pixels with the canvas scale), and saves
  `docfx/images/controls/<demo>.png`. The repository root is found by walking up from the executable to the folder
  containing `.git`.
- **MonoGame:** the sample registers no callback, and F9 does nothing there.
- **Release builds:** no capture code is compiled.

### 9. Game Studio verification (Plan 1)

- **What I prepare:**
  - a startup-script snippet (in `Snippets.Stride`): a `StartupScript` whose `Start()` casts `Game` to `Game` and
    calls `UseIcyUI()`, then `GetIcyConfiguration()`, `UseDefaultTheme()`, creates a `Canvas` and adds it to the
    overlay;
  - step-by-step instructions.
- **Ivan's check:** create a new Game Studio project, add the `IcyUI.Stride` package from a local feed (CI's
  `packages` artifact or a local pack), attach the script to an entity in the default scene, and run it.
- **Success:** the canvas draws over the 3D scene, and a button responds to the mouse and keyboard.
- **If it fails:** the fix to `IcyUI.Stride` is part of Plan 1, and the Getting started page documents only the
  verified path.
- **Out of scope:** a ready-made, code-free Game Studio component in `IcyUI.Stride`. It depends on whether Game Studio
  lists script types from a NuGet assembly, which is unknown.

### 10. Links and repository docs (Plan 1)

- **Package metadata:** `PackageProjectUrl` becomes `https://icyui.dev`. The four package READMEs link to the docs
  (`https://icyui.dev`, with the matching Getting started page for the engine packages). The "Source" and "Issues"
  links to GitHub stay.
- **The root `README.md` is refreshed:**
  - status: alpha, with the NuGet badge/links and the docs link;
  - engine table: MonoGame is backend-neutral;
  - features;
  - building;
  - history.
- **`CLAUDE.md`** gets a "Documentation" subsection:
  - the layout above and the local build/preview commands;
  - snippet and translation rules;
  - "each plan's pages ship in both languages".

### 11. Ivan's one-time setup (documented in `CLAUDE.md`)

1. **Cloudflare:** buy `icyui.dev`, with auto-renew on, using a long-lived personal email address and 2FA.
2. **Cloudflare DNS for `icyui.dev`:** set all records to "DNS only" (grey cloud).
   - `A` records: `185.199.108.153`, `185.199.109.153`, `185.199.110.153`, `185.199.111.153`;
   - `AAAA` records: `2606:50c0:8000::153`, `2606:50c0:8001::153`, `2606:50c0:8002::153`, `2606:50c0:8003::153`;
   - check these against GitHub's "Managing a custom domain for your GitHub Pages site" docs at setup time.
3. **GitHub domain verification:** in your profile's **Settings → Pages**, add `icyui.dev` and create the `TXT`
   record GitHub shows.
4. **Repository Pages settings:** in **Settings → Pages**, set the source to "GitHub Actions" and the custom domain to
   `icyui.dev`. Once the certificate is issued, tick "Enforce HTTPS".

Until DNS and the certificate are live, the site works at `https://ioexcept10n.github.io/IcyUI/`. The `.dev` TLD
requires HTTPS, so the custom domain won't load before the certificate exists.

## Plans

**Plan 1, the alpha.1 milestone:**
1. **DocFX tooling:** the local tool; the two-language skeleton (`docfx.en.json`, `docfx.ru.json`, the tocs, the
   switcher); the strict builds; placeholder cleanup; `.gitignore`.
2. **Snippets:** the two snippet projects and `DocsMarkupSnippetTests`.
3. **Deployment:** `docs.yml`, `CNAME` and `check-translations.sh` (with local fixtures).
4. **Game Studio verification:** the script and the steps, Ivan's smoke test, and any `IcyUI.Stride` fix.
5. **Content, English and Russian:** Home, Installation, Getting started for Stride and for MonoGame, Your first
   screen. The Home hero image is one screenshot Ivan takes manually. Plus the Russian glossary.
6. **The markup reference at `/markup`,** English and Russian, with the XSDs.
7. **Links and repo docs:** the package links, the root `README.md`, and `CLAUDE.md`'s Documentation section and
   one-time setup.

**Plan 2:** the F9 screenshot helper (Stride); Concepts (9 pages); the controls catalog (index plus ~25 pages). All in
both languages.

**Plan 3:** Guides and Project, in both languages. "Localizing your game UI" is included if alpha.2's localization has
landed.

## Testing

- **Builds:**
  - both DocFX builds pass with `--warningsAsErrors`;
  - the snippet projects compile in the solution build;
  - `DocsMarkupSnippetTests` passes, and fails when a snippet uses a nonexistent property (checked once with a
    temporary broken snippet).
- **`check-translations.sh`,** against fixtures:
  - fresh: no warning;
  - stale: a warning with the current hash;
  - stub: listed, no warning;
  - orphan Russian page: a warning.
- **Local preview** (`docfx serve`):
  - `/markup/` renders the markup reference;
  - the EN/RU switcher maps pages both ways;
  - the Russian pages' API links resolve to English API pages;
  - the `CNAME` is in `_site/`.
- **Lint:** `actionlint` + `shellcheck` on `docs.yml` and the new script.
- **Ivan's checks (not claimed):**
  - the Game Studio smoke test;
  - the first deploy is reachable at `github.io`, and then at `https://icyui.dev`;
  - his review of every page in both languages.

## Performance

- **No runtime cost to the libraries.** The screenshot helper is `#if DEBUG` only, and snippets aren't shipped.
- **CI:** one docs job of a few minutes, only when docs or library sources change. The snippet projects add seconds to
  the regular CI build.
- **Site:** static HTML with a client-side search index per language.

## Known limitations

- DocFX's built-in search uses an English stemmer, so Russian search matches exact word forms only.
- MonoGame has no screenshot capture until a later pass.

## Out of scope

- Library localization (its own sub-project, alpha.2).
- Versioned docs, analytics, PDF output, and better multilingual search.
- A code-free Game Studio component.
- Languages beyond English and Russian.
