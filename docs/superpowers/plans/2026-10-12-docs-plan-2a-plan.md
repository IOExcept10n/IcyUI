# Docs Plan 2a: deploy on tags, navigation, capture tool, controls catalog — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The docs site deploys only on `v*` tags, every section is reachable from a navbar and a sidebar, control
screenshots are captured from the pages' own markup snippets, and the controls catalog (an overview plus 12 family
pages) exists in English and Russian.

**Architecture:** `docs.yml` keeps building on every push and PR and deploys only for tags. The root `toc.yml` lists
all sections; each section folder has its own `toc.yml` (sidebar) and an `index.md`. A new code-only Stride app,
`sources/IcyUI.Docs.Capture`, loads every `docfx/snippets/markup/controls/*.xml` with the engine defaults, renders it
at scale 1.0, crops the back buffer to the element and saves `docfx/images/controls/<name>.png`, recording each
snippet's hash in a manifest that `build-docs.sh` checks. Pages are written in English first with Russian stubs;
after Ivan's review, one task translates them.

**Tech Stack:** DocFX (modern template), GitHub Actions, bash, Stride 4.4 (code-only game), IcyUI markup, xUnit.

**Spec:** `docs/superpowers/specs/2026-10-12-docs-plans-2-3-design.md` (and, where it isn't superseded,
`docs/superpowers/specs/2026-10-10-docs-site-design.md`).

## Global Constraints

- Docs deploy only when a `v*` tag is pushed; pushes and PRs only build.
- Every English page ships with a Russian page or a Russian stub (`translation: stub`); `/ru/` never 404s.
- A Russian page records `source-hash:` = `tr -d '\r' < docfx/en/<page>.md | sha256sum | cut -c1-12`.
- Russian follows `docfx/ru/_glossary.md`; code, type names and markup stay English.
- Code examples longer than a line come from snippet files: markup from `docfx/snippets/markup/**/*.xml` (loaded by
  `DocsMarkupSnippetTests`), C# from `#region`s in `sources/IcyUI.Docs.Snippets.Stride`/`.MonoGame` (namespaces `MyGame.*`).
- Both DocFX builds pass with zero warnings (`bash .github/scripts/build-docs.sh`).
- Shipped behaviour only; engine gaps in `> [!NOTE]` boxes. Members are linked with `<xref:…>`, not listed.
- Tone: second person, present tense, short paragraphs; the reader knows their engine, not necessarily WPF.
- **A library bug found at any step stops the plan; ask Ivan.** No library code changes in this plan. A snippet that
  fails because this plan wrote a wrong attribute name or value is a plan error: fix the snippet from the control's
  XML docs and ledger a ruling. A control that can't be expressed in markup at all is a stop.
- `IcyUI.Docs.Capture` is `IsPackable=false` and not StyleCop'd (it isn't shipped); `MonoGame Sample` conventions
  don't apply to it.
- Capture output: `docfx/images/controls/<snippet name>.png`, scale 1.0, 16 px padding, backdrop `#1A1A1F`
  (the Stride Sample's clear color 0.1/0.1/0.12).

## Review Focus

1. **A tag pushed on a commit that changes no docs path must still deploy.** GitHub doesn't evaluate `paths` filters
   for tag pushes; the workflow must not add a condition that re-introduces one. Pinned in Task 1, Step 3 (a grep
   that the deploy condition depends only on the ref).
2. **A fresh clone must not report every screenshot as stale.** Git doesn't preserve modification times, so the
   freshness check compares snippet content hashes with the capture manifest, not file times. Pinned in Task 4,
   Step 9.
3. **A snippet whose root stretches (a `Window`, a `Grid`) must still be captured at its own size,** not as the whole
   1280×800 viewport. The tool forces the root to Left/Top alignment. Pinned in Task 4, Step 8 (every PNG is smaller
   than the viewport).
4. **A snippet that fails to load must fail the capture run with its name,** not write a partial manifest that hides
   it. Pinned in Task 4, Step 7.
5. **Russian pages must link to Russian sections,** not fall back to English ones. Pinned in Task 2's navigation
   check, which renders `/ru/` pages and asserts `/ru/concepts/` etc.

---

## File structure

| File | Task | Responsibility |
|---|---|---|
| `.github/workflows/docs.yml` | 1 | build on push/PR/tag, deploy on `v*` tags only |
| `.github/scripts/build-docs.sh` | 1, 4 | preview hint; screenshot freshness warning |
| `CLAUDE.md` | 1, 4 | deploy-on-tag, one-time `github-pages` tag rule, local preview, capture command |
| `docfx/en/toc.yml`, `docfx/ru/toc.yml` | 2 | navbar |
| `docfx/{en,ru}/{concepts,controls,guides,project}/{index.md,toc.yml}` | 2 | section landing + sidebar |
| `docfx/{en,ru}/index.md` | 2 | Home link cards |
| `.github/scripts/check-navigation.sh` | 2 | local headless navbar/sidebar check |
| `sources/IcyUI.Tests/Docs/DocsSnippetViewModel.cs` | 3 | snippet data context (lists, tabs, settings) |
| `sources/IcyUI.Tests/Docs/DocsSnippetSettings.cs` | 3 | PropertyGrid target for the editors snippet |
| `docfx/snippets/markup/controls/*.xml` (12) | 3 | the controls snippets |
| `sources/IcyUI.Docs.Capture/*` | 4 | the capture tool |
| `docfx/images/controls/*.png`, `docfx/images/controls/captured.txt` | 4 | screenshots + hash manifest |
| `docfx/en/controls/*.md` (13) | 5 | English pages |
| `docfx/ru/controls/*.md` (13), `docfx/ru/_glossary.md` | 7 | Russian pages |

---

### Task 1: Docs CI deploys on tags

**Files:**
- Modify: `.github/workflows/docs.yml`
- Modify: `.github/scripts/build-docs.sh` (last lines)
- Modify: `CLAUDE.md` (Release steps; Documentation; one-time hosting setup)

**Interfaces:**
- Produces: the deploy condition `startsWith(github.ref, 'refs/tags/v')`, used by nothing else.

- [ ] **Step 1: Triggers.** In `docs.yml`, `on.push` becomes:

```yaml
on:
  push:
    branches: [platform-independent]
    tags: ['v*']
    paths: ['docfx/**', 'sources/IcyUI*/**', '.config/dotnet-tools.json', '.github/scripts/build-docs.sh', '.github/scripts/check-translations.sh', '.github/workflows/docs.yml']
  pull_request:
    paths: ['docfx/**', 'sources/IcyUI*/**', '.config/dotnet-tools.json', '.github/scripts/build-docs.sh', '.github/scripts/check-translations.sh', '.github/workflows/docs.yml']
```

Update the header comment to: "Builds the documentation site (English + Russian) on pushes and pull requests, and
deploys it to GitHub Pages (https://icyui.dev) when a release tag (v*) is pushed. Path filters don't apply to tag
pushes, so every release deploys."

- [ ] **Step 2: Conditions.** Replace both `if: github.event_name == 'push'` (the upload step and the `deploy` job) with
`if: startsWith(github.ref, 'refs/tags/v')`.

- [ ] **Step 3: Verify.**

Run: `grep -n "if:" .github/workflows/docs.yml`
Expected: exactly two lines, both `if: startsWith(github.ref, 'refs/tags/v')`.

Run actionlint. If `actionlint` isn't on PATH, download the release archive for this machine's architecture
(`actionlint_<version>_windows_amd64.zip` or `_windows_arm64.zip` from https://github.com/rhysd/actionlint/releases)
into a new empty temp directory and run it from there:
`<tmp>/actionlint.exe .github/workflows/docs.yml`
Expected: no output, exit 0.

- [ ] **Step 4: Preview hint.** At the end of `build-docs.sh`, after the Russian build, add:

```bash
echo "Built docfx/_site. Preview it over HTTP: dotnet docfx serve docfx/_site"
echo "(A page opened from disk shows no navbar, sidebar or search: the template loads them with fetch.)"
```

- [ ] **Step 5: CLAUDE.md.**
  - Documentation section: replace "deploys on pushes to `platform-independent`" with "builds on pushes and PRs, and
    deploys when a `v*` tag is pushed". Under the local build line add: "Preview over HTTP with
    `dotnet docfx serve docfx/_site`; a page opened from disk has no navbar, sidebar or search."
  - Release, step 3: append "The Docs workflow deploys the site from the same tag; check that run too."
  - One-time hosting setup paragraph: append "Then in **Settings → Environments → github-pages**, add the deployment
    tag rule `v*` (pushes to branches only build)."

- [ ] **Step 6: Build.**

Run: `bash .github/scripts/build-docs.sh > /tmp/docs.log 2>&1; echo exit=$?; tail -3 /tmp/docs.log`
Expected: `exit=0`; the last two lines are the preview hint.

- [ ] **Step 7: Commit.**

```bash
git add .github/workflows/docs.yml .github/scripts/build-docs.sh CLAUDE.md
git commit -m "Docs: deploy the site on release tags only; say how to preview it locally"
```

---

### Task 2: Navigation: navbar, section sidebars, Home cards, and a navigation check

**Files:**
- Create: `.github/scripts/check-navigation.sh`
- Modify: `docfx/en/toc.yml`, `docfx/ru/toc.yml`, `docfx/en/index.md`, `docfx/ru/index.md`
- Create: `docfx/en/{concepts,controls,guides,project}/index.md` and `toc.yml`; the same four folders under `docfx/ru/`

**Interfaces:**
- Produces: `bash .github/scripts/check-navigation.sh [section ...]` — exit 0 when every checked page renders the
  navbar and its section sidebar, 1 otherwise. Tasks 5, 7 and 8 run it.
- Produces: `docfx/en/controls/toc.yml` with the entry order Task 5 fills.

- [ ] **Step 1: Write the check (the failing test).** `.github/scripts/check-navigation.sh`:

```bash
#!/usr/bin/env bash
# Local check that the built site's navigation works: serves docfx/_site over HTTP, renders pages in headless Edge
# and asserts the rendered DOM has the navbar sections, the page's section sidebar and the EN/RU link. Not run in CI
# (it needs a browser). Usage: check-navigation.sh   (after build-docs.sh)
set -euo pipefail

port=8089
sections=(concepts controls guides project)
edge="/c/Program Files (x86)/Microsoft/Edge/Application/msedge.exe"
[[ -x "$edge" ]] || edge="/c/Program Files/Microsoft/Edge/Application/msedge.exe"
[[ -x "$edge" ]] || { echo "Microsoft Edge not found"; exit 2; }

dotnet docfx serve docfx/_site --port "$port" > /dev/null 2>&1 &
server=$!
profile=$(mktemp -d)
trap 'kill "$server" 2>/dev/null || true; rm -rf "$profile"' EXIT
for _ in $(seq 1 30); do curl -s -o /dev/null "http://localhost:$port/" && break; sleep 1; done

failures=0
render() {
  "$edge" --headless=new --disable-gpu --user-data-dir="$profile" --virtual-time-budget=8000 --dump-dom "$1" 2>/dev/null
}
expect() { # page-url, description, regex
  if ! grep -qE "$3" <<< "$dom"; then echo "FAIL $1: no $2"; failures=$((failures + 1)); fi
}

for lang in "" "ru/"; do
  for section in "${sections[@]}"; do
    url="http://localhost:$port/${lang}${section}/index.html"
    dom=$(render "$url")
    for other in "${sections[@]}"; do
      expect "$url" "navbar link to ${lang}${other}/" "class=\"nav-link[^>]*href=\"[^\"]*/${lang}${other}/"
    done
    # The navbar renders absolute hrefs (http://…/concepts/index.html); the sidebar renders relative ones, so a
    # relative href="index.html" can only come from the section's own sidebar (its Overview entry).
    expect "$url" "sidebar entry" "href=\"index\.html\""
    expect "$url" "language link" "title=\"(English|Русский)\""
  done
done

if ((failures > 0)); then echo "$failures navigation check(s) failed"; exit 1; fi
echo "Navigation: all pages render the navbar, their sidebar and the language link"
```

Note: the English navbar's links must match `/concepts/` etc. without `ru/`; for Russian pages the regex requires
`/ru/concepts/`. (`lang` is empty for English.)

- [ ] **Step 2: Run it to see it fail.**

Run: `bash .github/scripts/build-docs.sh > /tmp/docs.log 2>&1; bash .github/scripts/check-navigation.sh; echo exit=$?`
Expected: `FAIL` lines (no `concepts/index.html` exists yet; the navbar has no Concepts link), `exit=1`.

- [ ] **Step 3: Navbar.** `docfx/en/toc.yml`:

```yaml
- name: Getting started
  href: getting-started/
- name: Concepts
  href: concepts/
- name: Controls
  href: controls/
- name: Guides
  href: guides/
- name: Markup
  href: markup/index.md
- name: API
  href: ../api/
- name: Project
  href: project/
```

`docfx/ru/toc.yml`:

```yaml
- name: Начало работы
  href: getting-started/
- name: Концепции
  href: concepts/
- name: Элементы управления
  href: controls/
- name: Руководства
  href: guides/
- name: Разметка
  href: markup/index.md
- name: API (англ.)
  href: /api/index.html
- name: О проекте
  href: project/
```

- [ ] **Step 4: Section folders.** For each section, create `index.md` and `toc.yml` in `docfx/en/<section>/`.

`concepts/toc.yml`:
```yaml
- name: Overview
  href: index.md
```
`guides/toc.yml` and `project/toc.yml`: the same single entry. `controls/toc.yml` (Task 5 adds the families below
`Overview` in this order: Buttons, Text, Images and icons, Range controls, Lists and selection, Tree view, Tabs and
navigation, Expander and split pane, Scrolling, Panels, Dialogs and windows, Color and property editors):
```yaml
- name: Overview
  href: index.md
```

Stub `index.md` pages (English), one per section:

`concepts/index.md`:
```markdown
# Concepts

The ideas behind IcyUI: the canvas and its host, configuration, layout, data binding, styles and themes, templates,
input, animations, scaling, assets and fonts, and diagnostics.

> [!NOTE]
> These pages are being written. Until then, the [API reference](../api/index.md) documents every type.
```
`controls/index.md`:
```markdown
# Controls

Every control in IcyUI, grouped by family, with a screenshot, minimal markup and its keyboard, gamepad and touch
behaviour.

> [!NOTE]
> These pages are being written. Until then, the [API reference](../api/index.md) documents every control.
```
`guides/index.md`:
```markdown
# Guides

Step-by-step guides for common tasks: a game menu, custom controls and themes, hot reload and the in-game editor,
gamepad-friendly UI, and writing an engine integration.

> [!NOTE]
> These pages are being written.
```
`project/index.md`:
```markdown
# Project

IcyUI's status and roadmap, known issues, the changelog, how to build it from source, and its history and license.

> [!NOTE]
> These pages are being written. Known issues are listed in the package READMEs until then.
```

Russian: the same four folders under `docfx/ru/`. `toc.yml` entries use `name: Обзор`. Each `index.md` is a stub:

```markdown
---
translation: stub
---

# <Концепции | Элементы управления | Руководства | О проекте>

Эта страница ещё не переведена. Читайте [английскую версию](https://icyui.dev/<section>/index.html).
```

- [ ] **Step 5: Home cards.** In `docfx/en/index.md`, replace the "Where to go next" list with:

```markdown
## Where to go next

<div class="row row-cols-1 row-cols-md-3 g-3 mb-4">
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="getting-started/installation.md"><strong>Getting started</strong><span>Install IcyUI and put a first screen in a Stride or MonoGame game.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="concepts/index.md"><strong>Concepts</strong><span>Layout, binding, styles, input and the rest of the model.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="controls/index.md"><strong>Controls</strong><span>Every control, with screenshots and markup.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="guides/index.md"><strong>Guides</strong><span>Menus, custom controls, themes, the editor.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="markup/index.md"><strong>Markup reference</strong><span>The XML dialect for describing UI.</span></a></div>
  <div class="col"><a class="card h-100 p-3 text-decoration-none" href="api/index.md"><strong>API reference</strong><span>Every public type and member.</span></a></div>
</div>
```

If DocFX doesn't rewrite `.md` hrefs inside raw HTML (check the built `_site/index.html`), use the `.html` targets
instead (`getting-started/installation.html`, …) and ledger it. `docfx/ru/index.md` gets the same block with Russian
titles and text (Начало работы, Концепции, Элементы управления, Руководства, Справочник по разметке, Справочник API)
and the API card linking `https://icyui.dev/api/index.html`. Update the Russian page's `source-hash` only if it's a
full translation (check its front matter); if it's a stub, leave it.

- [ ] **Step 6: Run the check.**

Run: `bash .github/scripts/build-docs.sh > /tmp/docs.log 2>&1; echo build=$?; bash .github/scripts/check-navigation.sh; echo exit=$?; bash .github/scripts/check-translations.sh | tail -1`
Expected: `build=0`; the "Navigation: all pages render…" line, `exit=0`; translations: 4 new stubs, no stale or
missing pages.

- [ ] **Step 7: Commit.**

```bash
git add .github/scripts/check-navigation.sh docfx/en docfx/ru
git commit -m "Docs: a navbar for every section, section sidebars, Home cards, and a local navigation check"
```

---

### Task 3: Controls snippets and their data

**Files:**
- Modify: `sources/IcyUI.Tests/Docs/DocsSnippetViewModel.cs`
- Create: `sources/IcyUI.Tests/Docs/DocsSnippetSettings.cs`
- Create: `docfx/snippets/markup/controls/{buttons,text,images,range,lists,tree-view,navigation,expander-split,scrolling,panels,dialogs,editors}.xml`
- Modify: `sources/IcyUI.Tests/Docs/DocsMarkupSnippetTests.cs`

**Interfaces:**
- Produces: `DocsSnippetViewModel` (namespace `Icy.Tests.Docs`) with `Title`, `PlayCommand`, `QuitCommand` (existing)
  plus `IReadOnlyList<string> Levels`, `IReadOnlyList<string> Difficulties`, `string PlayerName`,
  `IReadOnlyList<TabItem> Tabs`, `DocsSnippetSettings Settings`. Task 4 links this file and `DocsSnippetSettings.cs`
  into the capture tool, so neither may depend on test-only types.

- [ ] **Step 1: Failing tests.** Add to `DocsMarkupSnippetTests`:

```csharp
        [Fact]
        public void ControlsSnippets_OneForEveryFamily()
        {
            string[] families = ["buttons", "text", "images", "range", "lists", "tree-view", "navigation", "expander-split", "scrolling", "panels", "dialogs", "editors"];

            foreach (string family in families)
                Assert.True(File.Exists(Path.Combine(SnippetRoot, "controls", family + ".xml")), $"No controls/{family}.xml snippet.");
        }

        [Fact]
        public void ListsSnippet_ShowsTheViewModelsLevels()
        {
            var root = (StackPanel)Load(Path.Combine("controls", "lists.xml"));
            root.DataContext = new DocsSnippetViewModel();

            var listBox = Assert.IsType<ListBox>(root.Children[1]);
            Assert.Equal(new[] { "Forest", "Caves", "Castle" }, listBox.ItemsSource!.Cast<string>());
        }
```

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~DocsMarkupSnippetTests"`
Expected: FAIL (no snippets; `ListsSnippet…` throws on the missing file).

- [ ] **Step 2: View model data.** `DocsSnippetViewModel.cs` gains (keep the existing members and doc comment;
add `using Icy.UI.Controls;`):

```csharp
        public IReadOnlyList<string> Levels { get; } = ["Forest", "Caves", "Castle"];

        public IReadOnlyList<string> Difficulties { get; } = ["Easy", "Normal", "Hard"];

        public string PlayerName { get; set; } = "Player 1";

        public IReadOnlyList<TabItem> Tabs { get; } =
        [
            new TabItem { Header = "Video", Content = "Resolution: 1920 × 1080" },
            new TabItem { Header = "Audio", Content = "Master volume: 70%" },
            new TabItem { Header = "Controls", Content = "Invert Y axis: off" },
        ];

        public DocsSnippetSettings Settings { get; } = new();
```

`DocsSnippetSettings.cs`:

```csharp
namespace Icy.Tests.Docs
{
    /// <summary>Graphics settings shown by the editors snippet's PropertyGrid.</summary>
    public sealed class DocsSnippetSettings
    {
        public enum QualityLevel
        {
            Low,
            Medium,
            High,
        }

        public bool VSync { get; set; } = true;

        public int FrameRateLimit { get; set; } = 144;

        public QualityLevel Quality { get; set; } = QualityLevel.High;
    }
}
```

- [ ] **Step 3: The snippets.** Each file starts with a one-line comment saying what it shows. Exact contents:

`buttons.xml`
```xml
<!-- Button runs a command; ToggleButton and CheckBox keep an on/off state in IsChecked. -->
<StackPanel Orientation="Vertical">
  <Button Margin="0,0,0,8" Command="{Binding PlayCommand}">Play</Button>
  <ToggleButton Margin="0,0,0,8" IsChecked="True">Music</ToggleButton>
  <CheckBox IsChecked="True">Subtitles</CheckBox>
</StackPanel>
```

`text.xml`
```xml
<!-- TextBlock shows text; TextBox edits it. -->
<StackPanel Orientation="Vertical" Width="240">
  <TextBlock FontSize="20" Margin="0,0,0,8">Player name</TextBlock>
  <TextBox Text="{Binding PlayerName, Mode=TwoWay}" />
</StackPanel>
```

`images.xml`
```xml
<!-- Icon draws a small vector glyph; Image shows a texture (see the page for loading one). -->
<StackPanel Orientation="Horizontal">
  <Icon Kind="Checkmark" Width="24" Height="24" Margin="0,0,12,0" />
  <Icon Kind="ChevronLeft" Width="24" Height="24" Margin="0,0,12,0" />
  <Icon Kind="ChevronRight" Width="24" Height="24" Margin="0,0,12,0" />
  <Icon Kind="ChevronUp" Width="24" Height="24" Margin="0,0,12,0" />
  <Icon Kind="ChevronDown" Width="24" Height="24" />
</StackPanel>
```

`range.xml`
```xml
<!-- Slider picks a value in a range; ProgressBar shows one. -->
<StackPanel Orientation="Vertical" Width="240">
  <TextBlock Margin="0,0,0,4">Volume</TextBlock>
  <Slider Minimum="0" Maximum="100" Value="70" Margin="0,0,0,16" />
  <TextBlock Margin="0,0,0,4">Loading</TextBlock>
  <ProgressBar Minimum="0" Maximum="100" Value="40" Height="8" />
</StackPanel>
```

`lists.xml`
```xml
<!-- ComboBox and ListBox show the items of ItemsSource and select one of them. -->
<StackPanel Orientation="Vertical" Width="240">
  <ComboBox ItemsSource="{Binding Levels}" SelectedIndex="1" Margin="0,0,0,8" />
  <ListBox ItemsSource="{Binding Levels}" SelectedIndex="0" Height="96" />
</StackPanel>
```

`tree-view.xml`
```xml
<!-- A static tree written in markup with TreeViewNode; bind ItemsSource for a tree built from your data. -->
<TreeView Width="240">
  <TreeViewNode Header="Settings" IsExpanded="True">
    <TreeViewNode Header="Video" />
    <TreeViewNode Header="Audio" />
    <TreeViewNode Header="Controls" IsExpanded="True">
      <TreeViewNode Header="Keyboard" />
      <TreeViewNode Header="Gamepad" />
    </TreeViewNode>
  </TreeViewNode>
</TreeView>
```

`navigation.xml`
```xml
<!-- TabControl shows the TabItems in ItemsSource as tabs, and the selected tab's Content below them. -->
<TabControl ItemsSource="{Binding Tabs}" SelectedIndex="0" Width="320" Height="140" />
```

`expander-split.xml`
```xml
<!-- Expander shows or hides its content under a header; SplitPane divides space between two panes. -->
<StackPanel Orientation="Vertical" Width="300">
  <Expander Header="Advanced" IsExpanded="True" Margin="0,0,0,12">
    <TextBlock>Shadow quality: high</TextBlock>
  </Expander>
  <SplitPane Orientation="Horizontal" SplitterPosition="0.4" Height="80">
    <SplitPane.First>
      <TextBlock Margin="8">Scene</TextBlock>
    </SplitPane.First>
    <SplitPane.Second>
      <TextBlock Margin="8">Properties</TextBlock>
    </SplitPane.Second>
  </SplitPane>
</StackPanel>
```

`scrolling.xml`
```xml
<!-- ScrollViewer scrolls content larger than itself. -->
<ScrollViewer Width="240" Height="120">
  <StackPanel Orientation="Vertical">
    <TextBlock>1. Forest</TextBlock>
    <TextBlock>2. Caves</TextBlock>
    <TextBlock>3. Castle</TextBlock>
    <TextBlock>4. Tower</TextBlock>
    <TextBlock>5. Swamp</TextBlock>
    <TextBlock>6. Volcano</TextBlock>
    <TextBlock>7. Glacier</TextBlock>
    <TextBlock>8. Throne room</TextBlock>
  </StackPanel>
</ScrollViewer>
```

`panels.xml`
```xml
<!-- Grid places children in rows and columns; StackPanel stacks them. -->
<Grid Width="300">
  <Grid.ColumnDefinitions>
    <ColumnDefinition Width="Auto" />
    <ColumnDefinition Width="*" />
  </Grid.ColumnDefinitions>
  <Grid.RowDefinitions>
    <RowDefinition Height="Auto" />
    <RowDefinition Height="Auto" />
  </Grid.RowDefinitions>
  <TextBlock Grid.Row="0" Grid.Column="0" Margin="0,0,12,8">Name</TextBlock>
  <TextBox Grid.Row="0" Grid.Column="1" Margin="0,0,0,8" Text="{Binding PlayerName}" />
  <TextBlock Grid.Row="1" Grid.Column="0" Margin="0,0,12,0">Difficulty</TextBlock>
  <ComboBox Grid.Row="1" Grid.Column="1" ItemsSource="{Binding Difficulties}" SelectedIndex="1" />
</Grid>
```

`dialogs.xml`
```xml
<!-- A Window traps keyboard and gamepad focus while open; Dialog and MessageBox are shown from code (see the page). -->
<Window Width="300">
  <StackPanel Orientation="Vertical" Padding="16">
    <TextBlock Margin="0,0,0,12">Quit to the main menu?</TextBlock>
    <StackPanel Orientation="Horizontal">
      <Button Margin="0,0,8,0" Command="{Binding QuitCommand}">Quit</Button>
      <Button>Cancel</Button>
    </StackPanel>
  </StackPanel>
</Window>
```

`editors.xml`
```xml
<!-- ColorPicker edits a color; PropertyGrid edits an object's public properties. -->
<StackPanel Orientation="Horizontal">
  <ColorPicker SelectedColor="#FF3C78D8" Margin="0,0,16,0" />
  <PropertyGrid Target="{Binding Settings}" Width="280" />
</StackPanel>
```

- [ ] **Step 4: Run the tests.**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~DocsMarkupSnippetTests" > /tmp/snip.log 2>&1; grep -E "Passed!|Failed!|\[FAIL\]" /tmp/snip.log`
Expected: all pass. A failure from a wrong attribute name or value written in this plan is fixed from the control's
XML docs (ledger a ruling). A control that can't be written in markup at all stops the plan: ask Ivan.

- [ ] **Step 5: Mutation check.** Temporarily change `IsChecked="True"` in `buttons.xml` to `IsChekced="True"`, run
the same command, and confirm `EveryMarkupSnippet_Loads(controls\buttons.xml)` fails; restore the file.

- [ ] **Step 6: Full suite.**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj > /tmp/all.log 2>&1; grep -E "Passed!|Failed!" /tmp/all.log`
Expected: `Failed: 0` (Total = 1880 + 2 new facts + 12 new theory rows).

- [ ] **Step 7: Commit.**

```bash
git add sources/IcyUI.Tests/Docs docfx/snippets/markup/controls
git commit -m "Docs: a markup snippet for every control family, with data to bind to"
```

---

### Task 4: The capture tool

**Files:**
- Create: `sources/IcyUI.Docs.Capture/IcyUI.Docs.Capture.csproj`, `Program.cs`, `CaptureGame.cs`, `SnippetCapture.cs`
- Modify: `sources/IcyUI.sln` (add the project)
- Modify: `.github/scripts/build-docs.sh` (freshness warning)
- Modify: `CLAUDE.md` (layout table row; Documentation: the capture command)
- Create (generated): `docfx/images/controls/*.png`, `docfx/images/controls/captured.txt`

**Interfaces:**
- Consumes: `DocsSnippetViewModel`, `DocsSnippetSettings` (Task 3), linked as source files.
- Produces: `dotnet run --project sources/IcyUI.Docs.Capture` → exit 0 and one PNG per controls snippet, or exit 1
  with `FAILED <snippet>: <message>` on stderr. Manifest format, one line per snippet, sorted:
  `<name> <first 12 hex of sha256 of the snippet with CRs removed>` — the same hash `build-docs.sh` computes with
  `tr -d '\r' < file | sha256sum | cut -c1-12`.

- [ ] **Step 1: Project.** `IcyUI.Docs.Capture.csproj` (modelled on `Stride Sample`):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>$(IcyTargetFramework)-windows</TargetFramework>
    <RootNamespace>Icy.Docs.Capture</RootNamespace>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Stride.Core" />
    <PackageReference Include="Stride.Engine" />
    <PackageReference Include="Stride.Graphics" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\IcyUI\IcyUI.csproj" />
    <ProjectReference Include="..\IcyUI.Stride\IcyUI.Stride.csproj" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="..\IcyUI.Tests\Docs\DocsSnippetViewModel.cs" Link="Snippets\DocsSnippetViewModel.cs" />
    <Compile Include="..\IcyUI.Tests\Docs\DocsSnippetSettings.cs" Link="Snippets\DocsSnippetSettings.cs" />
  </ItemGroup>
</Project>
```

If `Stride Sample.csproj` has other settings a code-only Stride game needs (an `app.manifest` for DPI, Stride build
properties), copy them; the sample is the reference. `dotnet sln sources/IcyUI.sln add sources/IcyUI.Docs.Capture/IcyUI.Docs.Capture.csproj`.

- [ ] **Step 2: `SnippetCapture.cs`** (pure helpers, no Stride types except in `Crop`):

```csharp
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Stride.Graphics;

namespace Icy.Docs.Capture
{
    /// <summary>File locations, hashing and cropping for the capture run.</summary>
    internal static class SnippetCapture
    {
        public const int Padding = 16;

        /// <summary>Walks up from <paramref name="start"/> to the folder that contains <c>.git</c>.</summary>
        public static string FindRepositoryRoot(string start)
        {
            for (DirectoryInfo? dir = new(start); dir != null; dir = dir.Parent)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, ".git")) || File.Exists(Path.Combine(dir.FullName, ".git")))
                    return dir.FullName;
            }

            throw new DirectoryNotFoundException($"No .git folder above {start}.");
        }

        /// <summary>The first 12 hex digits of the SHA-256 of the file with CRs removed, as build-docs.sh computes it.</summary>
        public static string Hash(string file)
        {
            string text = File.ReadAllText(file).Replace("\r", string.Empty, StringComparison.Ordinal);
            return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..12];
        }

        /// <summary>The element's bounds plus <see cref="Padding"/>, clipped to the back buffer.</summary>
        public static Rectangle CaptureArea(Rectangle bounds, Size backBuffer)
        {
            Rectangle area = Rectangle.Inflate(bounds, Padding, Padding);
            return Rectangle.Intersect(area, new Rectangle(Point.Empty, backBuffer));
        }

        /// <summary>Copies <paramref name="area"/> of <paramref name="source"/> into a new image of the same format.</summary>
        public static Image Crop(Image source, Rectangle area)
        {
            var cropped = Image.New2D(area.Width, area.Height, 1, source.Description.Format);
            PixelBuffer from = source.GetPixelBuffer(0, 0);
            PixelBuffer to = cropped.GetPixelBuffer(0, 0);
            for (int y = 0; y < area.Height; y++)
            {
                for (int x = 0; x < area.Width; x++)
                    to.SetPixel(x, y, from.GetPixel<uint>(area.X + x, area.Y + y));
            }

            return cropped;
        }
    }
}
```

(`Crop` assumes a 4-byte pixel format, which a Stride back buffer is; if `source.Description.Format.SizeInBytes()`
isn't 4, throw `NotSupportedException` naming the format.)

- [ ] **Step 3: `CaptureGame.cs`.**

```csharp
using System.Drawing;
using Icy.Configuration;
using Icy.Markup;
using Icy.Stride.Configuration;
using Icy.Tests.Docs;
using Icy.UI;
using Stride.Engine;
using Stride.Games;
using Stride.Graphics;
using Stride.Rendering.Compositing;
using Color4 = Stride.Core.Mathematics.Color4;

namespace Icy.Docs.Capture
{
    /// <summary>
    /// Renders every controls snippet once and saves it as a PNG. One snippet is shown at a time, for a few frames so
    /// layout and templates settle, then captured from the back buffer and removed.
    /// </summary>
    internal sealed class CaptureGame : Game
    {
        private const int SettleFrames = 3;

        private readonly Queue<string> pending = new();
        private readonly SortedDictionary<string, string> manifest = new(StringComparer.Ordinal);
        private string snippetFolder = string.Empty;
        private string imageFolder = string.Empty;
        private StrideIcyHost? host;
        private (string Name, UIElement Root)? current;
        private int framesLeft;

        public CaptureGame()
        {
            GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
            GraphicsDeviceManager.PreferredBackBufferHeight = 800;
        }

        protected override void BeginRun()
        {
            base.BeginRun();

            SceneSystem.GraphicsCompositor = new GraphicsCompositor
            {
                Game = new SceneRendererCollection { new ClearRenderer { Color = new Color4(0.1f, 0.1f, 0.12f, 1f) } },
            };
            SceneSystem.SceneInstance = new SceneInstance(Services, new Scene());
            host = this.UseIcyUI();
            host.MainCanvas.ScaleMode = UIScaleMode.None;

            string root = SnippetCapture.FindRepositoryRoot(AppContext.BaseDirectory);
            snippetFolder = Path.Combine(root, "docfx", "snippets", "markup", "controls");
            imageFolder = Path.Combine(root, "docfx", "images", "controls");
            Directory.CreateDirectory(imageFolder);
            foreach (string file in Directory.EnumerateFiles(snippetFolder, "*.xml").Order(StringComparer.Ordinal))
                pending.Enqueue(file);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (current != null || host == null)
                return;

            if (!pending.TryDequeue(out string? file))
            {
                File.WriteAllLines(Path.Combine(imageFolder, "captured.txt"), manifest.Select(x => $"{x.Key} {x.Value}"));
                Console.WriteLine($"Captured {manifest.Count} snippet(s) into {imageFolder}");
                Exit();
                return;
            }

            string name = Path.GetFileNameWithoutExtension(file);
            try
            {
                UIElement element = new MarkupLoader(host.Configuration).Load(File.ReadAllText(file), Path.GetFileName(file));
                element.DataContext = new DocsSnippetViewModel();
                element.HorizontalAlignment = HorizontalAlignment.Left;
                element.VerticalAlignment = VerticalAlignment.Top;
                element.Margin = new Thickness(SnippetCapture.Padding);
                host.MainCanvas.Add(element);
                current = (name, element);
                framesLeft = SettleFrames;
            }
            catch (Exception ex)
            {
                Fail(name, ex);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            if (current is not { } shown || --framesLeft > 0)
                return;

            try
            {
                Texture backBuffer = GraphicsDevice.Presenter.BackBuffer;
                Rectangle area = SnippetCapture.CaptureArea(shown.Root.ActualBounds, new Size(backBuffer.Width, backBuffer.Height));
                using Image frame = backBuffer.GetDataAsImage(GraphicsContext.CommandList);
                using Image cropped = SnippetCapture.Crop(frame, area);
                using (FileStream stream = File.Create(Path.Combine(imageFolder, shown.Name + ".png")))
                    cropped.Save(stream, ImageFileType.Png);
                manifest[shown.Name] = SnippetCapture.Hash(Path.Combine(snippetFolder, shown.Name + ".xml"));
            }
            catch (Exception ex)
            {
                Fail(shown.Name, ex);
                return;
            }

            host!.MainCanvas.Remove(shown.Root);
            current = null;
        }

        private void Fail(string name, Exception ex)
        {
            Console.Error.WriteLine($"FAILED {name}: {ex.Message}");
            Environment.ExitCode = 1;
            pending.Clear();
            current = null;
            Exit();
        }
    }
}
```

Notes for the implementer: the root's margin is inside `ActualBounds`' offset (the element sits at 16,16); the
capture area inflates the bounds by the same padding, so the PNG has 16 px of backdrop on every side. If
`Canvas.Remove` has a different name, use the canvas's removal API (`Canvas.cs`). If the Stride 4.4 API names differ
from these (`GraphicsDeviceManager`, `GetDataAsImage`, `Image.New2D`), adapt and ledger. On failure, nothing is
written to the manifest (Exit runs before the queue empties).

`Program.cs`:

```csharp
using Icy.Docs.Capture;

using var game = new CaptureGame();
game.Run();
return Environment.ExitCode;
```

- [ ] **Step 4: Build.**

Run: `dotnet build sources/IcyUI.Docs.Capture/IcyUI.Docs.Capture.csproj > /tmp/cap.log 2>&1; grep -E " error |Error\(s\)" /tmp/cap.log`
Expected: `0 Error(s)`.

- [ ] **Step 5: Freshness check (the failing test).** Add to `build-docs.sh`, before the DocFX builds:

```bash
# Control screenshots come from docfx/snippets/markup/controls by IcyUI.Docs.Capture, which records each snippet's
# hash in captured.txt. Warn (CI can't run the capture) when a snippet has no screenshot or changed since.
manifest=docfx/images/controls/captured.txt
for snippet in docfx/snippets/markup/controls/*.xml; do
  [[ -e "$snippet" ]] || continue
  name=$(basename "$snippet" .xml)
  hash=$(tr -d '\r' < "$snippet" | sha256sum | cut -c1-12)
  if [[ ! -f "docfx/images/controls/$name.png" ]] || ! grep -qx "$name $hash" "$manifest" 2>/dev/null; then
    echo "::warning file=$snippet::The screenshot for $name is missing or stale. Run: dotnet run --project sources/IcyUI.Docs.Capture"
  fi
done
```

Run: `bash .github/scripts/build-docs.sh 2>&1 | grep -c "screenshot for"`
Expected: `12` (no captures yet).

- [ ] **Step 6: Capture.**

Run: `dotnet run --project sources/IcyUI.Docs.Capture; echo exit=$?; ls docfx/images/controls; cat docfx/images/controls/captured.txt`
Expected: `Captured 12 snippet(s)…`, `exit=0`, 12 PNGs and 12 manifest lines. Open three PNGs (Read tool) and check
each shows its controls on the dark backdrop with padding. A control that renders wrongly (missing parts, wrong
layout) is a library bug: stop and ask Ivan.

- [ ] **Step 7: A broken snippet fails the run.** Copy `buttons.xml` to `controls/zz-broken.xml` with
`<Button Nonexistent="1"/>` as its only content, run the capture again, and expect `FAILED zz-broken: …` and
`exit=1`; `captured.txt` must be unchanged from Step 6 (`git diff --stat docfx/images/controls/captured.txt` empty).
Delete `zz-broken.xml` and its PNG if one was written.

- [ ] **Step 8: Every capture is smaller than the viewport.**

Run (PowerShell from bash):
`powershell -NoProfile -Command "Add-Type -AssemblyName System.Drawing; Get-ChildItem docfx/images/controls/*.png | ForEach-Object { \$i=[System.Drawing.Image]::FromFile(\$_.FullName); '{0} {1}x{2}' -f \$_.Name,\$i.Width,\$i.Height; \$i.Dispose() }"`
Expected: 12 lines; every width < 1280 and height < 800.

- [ ] **Step 9: Fresh clone doesn't warn.**

Run: `touch docfx/snippets/markup/controls/*.xml; bash .github/scripts/build-docs.sh 2>&1 | grep -c "screenshot for"`
Expected: `0` (hashes match; times don't matter).

- [ ] **Step 10: CLAUDE.md.** Layout table: add a row
`| IcyUI.Docs.Capture | Docs tool | Code-only Stride app: renders docfx/snippets/markup/controls/*.xml and saves the screenshots. Never packed. |`.
Documentation section: add "Control screenshots: `dotnet run --project sources/IcyUI.Docs.Capture` (Windows, needs a
GPU). `build-docs.sh` warns when a controls snippet changed since its screenshot."

- [ ] **Step 11: Solution build and tests.**

Run: `dotnet build sources/IcyUI.sln -c Release > /tmp/rel.log 2>&1; grep -E "warning|error" /tmp/rel.log | grep -v NU190 | grep -E "IcyUI\\\\|IcyUI.Design|IcyUI.MonoGame|IcyUI.Stride\\\\" | sort -u`
Expected: no lines (shipped libraries still warning-free). Tests as in Task 3, Step 6: `Failed: 0`.

- [ ] **Step 12: Commit.**

```bash
git add sources/IcyUI.Docs.Capture sources/IcyUI.sln .github/scripts/build-docs.sh CLAUDE.md docfx/images/controls
git commit -m "Docs: capture control screenshots from the markup snippets with IcyUI.Docs.Capture"
```

---

### Task 5: Controls pages (English) with Russian stubs

**Files:**
- Modify: `docfx/en/controls/index.md`, `docfx/en/controls/toc.yml`
- Create: `docfx/en/controls/{buttons,text,images,range,lists,tree-view,navigation,expander-split,scrolling,panels,dialogs,editors}.md`
- Create: the same 12 under `docfx/ru/controls/` as stubs
- Create (if a page needs C#): `sources/IcyUI.Docs.Snippets.Stride/Controls.cs` (`namespace MyGame.Controls…`, `#region`s)

**Interfaces:**
- Consumes: the snippets (Task 3) and PNGs (Task 4); `check-navigation.sh` (Task 2).

- [ ] **Step 1: `toc.yml`.**

```yaml
- name: Overview
  href: index.md
- name: Buttons
  href: buttons.md
- name: Text
  href: text.md
- name: Images and icons
  href: images.md
- name: Range controls
  href: range.md
- name: Lists and selection
  href: lists.md
- name: Tree view
  href: tree-view.md
- name: Tabs and navigation
  href: navigation.md
- name: Expander and split pane
  href: expander-split.md
- name: Scrolling
  href: scrolling.md
- name: Panels
  href: panels.md
- name: Dialogs and windows
  href: dialogs.md
- name: Color and property editors
  href: editors.md
```
Russian `toc.yml`: the same hrefs with names Обзор, Кнопки, Текст, Изображения и значки, Элементы диапазона, Списки и
выбор, Дерево, Вкладки и навигация, Expander и SplitPane, Прокрутка, Панели, Диалоги и окна, Редакторы цвета и
свойств.

- [ ] **Step 2: Family pages.** Each page follows this skeleton (headings exact; prose per the content standards):

```markdown
# <Family title>

![<alt text describing what's shown>](../../images/controls/<name>.png)

<Purpose and when to use it: 2–4 sentences.>

## Markup

[!code-xml[](../../snippets/markup/controls/<name>.xml)]

<One paragraph explaining the snippet.>

## Key properties

| Property | Description |
|---|---|
| <xref:Icy.UI.Controls.<Type>.<Property>> | <one line> |

## Events and commands

## Styling

<The implicit style's TargetType, the template's named parts if any, and the visual states with their names.>

## Keyboard, gamepad and touch

## <Member> 

<One subsection per family member, for what differs from the family.>

## See also
```

The facts each page must cover (from the code; check the control's XML docs and the default theme
`sources/IcyUI/Resources/Themes/DefaultTheme.xml` while writing):

| Page | Members | Must cover |
|---|---|---|
| buttons | Button, ToggleButton, CheckBox | `Command`; Button's default theme alignment is centered (a local `Stretch` fills the slot); `IsChecked`/`IsCheckedChanged`; CheckBox `CheckBrush`; text content becomes a `TextBlock`; visual states Hovered/Pressed/Focused |
| text | TextBlock, TextBox | `Text`, `FontFamily`, `FontSize`, `FontStyle`; the default family (Segoe UI with the engine defaults) and the missing-font warning; `TextChanged`; two-way binding of `Text` and the validation border for numeric binding errors |
| images | Image, Icon | `Image.Source` (an `IImage`, loaded through the asset pipeline — show a one-line C# load if a snippet project region is needed); `Icon.Kind` (the 5 `IconKind` values), `Stroke`, `StrokeThickness` |
| range | Slider, ProgressBar | `Minimum`/`Maximum`/`Value`, `ValueChanged`; `ThumbBrush`, `FillBrush`; arrow keys and gamepad on Slider |
| lists | ItemsControl, ListBox, ComboBox, Dropdown, WrapGrid | `ItemsSource`, `ItemTemplate`, `SelectedIndex`/`SelectedItem`, `SelectionChanged`, `DisplayMemberPath`, `IsOpen`; pooled containers (items are reused when scrolling); WrapGrid `ItemWidth`/`ItemHeight` |
| tree-view | TreeView | inline `TreeViewNode` vs `ItemsSource` + `ChildrenSelector`; `SelectedItem`, `ItemExpanded`/`ItemCollapsed`; `Indent`; arrow-key expand/collapse |
| navigation | TabControl, Frame, Page | tabs come from `ItemsSource` of `TabItem`s (`Header`, `Icon`, `Content`) — show the view model's `Tabs` in C# from the snippet project; Frame/Page navigation (link the markup reference's "Pages and navigation") |
| expander-split | Expander, SplitPane | `Header`, `IsExpanded`, `IsExpandedChanged`; `First`/`Second`, `Orientation`, `SplitterPosition`, `MinFirstSize`/`MinSecondSize`, `SplitterPositionChanged` |
| scrolling | ScrollViewer | `VerticalScrollMode`/`HorizontalScrollMode`, offsets, `PanningMode` (touch), `ScrollChanged` |
| panels | StackPanel, Grid | `Orientation`; `ColumnDefinitions`/`RowDefinitions` with `Auto`, `*` and pixel sizes; `Grid.Row`/`Grid.Column` attached properties; link Concepts › Layout (once it exists: `../concepts/index.md` for now) |
| dialogs | Dialog, MessageBox, Window | Window: `IsOpen`, focus trap, no pointer isolation; Dialog: modal overlay with backdrop, Escape closes; `Dialog<TResult>` and MessageBox shown from code (a C# region in `Snippets.Stride/Controls.cs`) |
| editors | ColorPicker, ColorPickerButton, PropertyGrid | `SelectedColor`, `ColorChanged`; ColorPickerButton opens a picker; PropertyGrid `Target`, how it picks editors per property type (bool, numbers, enums) |

Engine gaps go in notes: touch isn't available on MonoGame DesktopGL (scrolling, panning pages).

- [ ] **Step 3: Overview.** Replace the `controls/index.md` stub: one sentence, then a grid of the 12 screenshots,
each linking to its page:

```markdown
# Controls

IcyUI's controls, grouped into families that share their behaviour. Each page shows the family's markup, its key
properties, how to restyle it, and how it responds to keyboard, gamepad and touch.

<div class="row row-cols-2 row-cols-md-3 g-3">
  <div class="col"><a class="card h-100 p-2 text-decoration-none" href="buttons.md"><img src="../../images/controls/buttons.png" alt="A button, a toggle button and a check box" class="card-img-top"><span class="mt-2">Buttons</span></a></div>
  …one card per family, same pattern…
</div>
```
(Use `.html` hrefs if Task 2 found that raw-HTML `.md` links aren't rewritten.)

- [ ] **Step 4: Russian stubs.** For each of the 12 pages, `docfx/ru/controls/<name>.md`:

```markdown
---
translation: stub
---

# <Russian title from the toc>

Эта страница ещё не переведена. Читайте [английскую версию](https://icyui.dev/controls/<name>.html).
```

- [ ] **Step 5: Build and checks.**

Run: `bash .github/scripts/build-docs.sh > /tmp/docs.log 2>&1; echo build=$?; grep -c "screenshot for" /tmp/docs.log; bash .github/scripts/check-navigation.sh; bash .github/scripts/check-translations.sh | tail -1`
Expected: `build=0`; `0`; navigation passes; translations show the new stubs and no stale or missing pages. If a C#
region was added, `dotnet build sources/IcyUI.Docs.Snippets.Stride/IcyUI.Docs.Snippets.Stride.csproj -c Release` →
`0 Error(s)`.

- [ ] **Step 6: Commit.**

```bash
git add docfx/en/controls docfx/ru/controls sources/IcyUI.Docs.Snippets.Stride
git commit -m "Docs: the controls catalog: an overview and 12 family pages"
```

---

### Task 6: Ivan reviews the English pages (stop)

- [ ] **Step 1:** Build the site and start `dotnet docfx serve docfx/_site`. Tell Ivan the URL
(`http://localhost:8080/controls/index.html`) and the list of changed pages (Home, the four section stubs, the 13
controls pages).
- [ ] **Step 2:** Wait for Ivan's approval or edits. Apply his edits, rebuild, rerun `check-navigation.sh`, commit
("Docs: controls pages after review"), and ledger what changed. Re-capture if a snippet changed.

---

### Task 7: Russian pages

**Files:**
- Modify: `docfx/ru/controls/*.md` (13), `docfx/ru/_glossary.md`

- [ ] **Step 1: Glossary.** Add any terms the pages need that `_glossary.md` lacks (expected: `toggle button`,
`check box`, `slider`, `progress bar`, `tree view`, `tab`, `split pane`, `scroll viewer`, `dialog`, `window`,
`property grid`, `color picker`, `item template`, `selection`). Control type names stay English in code and in prose
the first time they name the type (`Button`), per the existing glossary's convention for `Canvas`.
- [ ] **Step 2: Translate** each approved English page into its Russian file, replacing the stub front matter with
`source-hash: <hash of the English page>` (compute with
`tr -d '\r' < docfx/en/controls/<name>.md | sha256sum | cut -c1-12`). Code blocks and snippet includes stay as they
are; image alt text is translated.
- [ ] **Step 3: Checks.**

Run: `bash .github/scripts/build-docs.sh > /tmp/docs.log 2>&1; echo build=$?; bash .github/scripts/check-translations.sh | tail -1; bash .github/scripts/check-navigation.sh`
Expected: `build=0`; controls pages all fresh (only the Concepts/Guides/Project stubs remain stubs); navigation passes.
- [ ] **Step 4: Commit.**

```bash
git add docfx/ru
git commit -m "Docs: the controls catalog in Russian"
```

---

### Task 8: Plan verification

- [ ] **Step 1:** `dotnet build sources/IcyUI.sln -c Release` — shipped libraries warning-free (audit warnings
excepted); `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj` — `Failed: 0`.
- [ ] **Step 2:** `bash .github/scripts/build-docs.sh` — exit 0, no "screenshot for" warnings.
- [ ] **Step 3:** `bash .github/scripts/check-navigation.sh` — passes.
- [ ] **Step 4:** `bash .github/scripts/check-translations.sh` — controls and Home fresh; stubs only for the Concepts,
Guides and Project overviews.
- [ ] **Step 5:** Report to Ivan: what's left for him (the `github-pages` tag rule; reading the Russian pages), and
that nothing is deployed until the alpha.1 tag.
