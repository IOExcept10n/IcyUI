# Docs Site Plan 1 (alpha.1) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship the first version of `https://icyui.dev` in English and Russian, built and deployed by CI, and point the packages and the README at it.
- **Infrastructure:** a strict two-language DocFX build, compiled and tested snippets, and a Pages deploy with a translation-staleness check.
- **Pages:** Home, Installation, Getting started for Stride and for MonoGame, Your first screen, and the markup reference at `/markup`.

**Architecture:**
- **Tooling:** DocFX 2.81.0 is a repo-local tool.
- **Builds:** `docfx/docfx.en.json` generates the API metadata (four shipped libraries) and builds `docfx/en/` into `docfx/_site/`. `docfx/docfx.ru.json` builds `docfx/ru/` into `docfx/_site/ru/` and resolves API xrefs through the English `xrefmap.yml`, re-based to `/`.
- **One build script:** `.github/scripts/build-docs.sh` runs the same steps locally and in `docs.yml`.
- **Snippets:** C# snippets live in two non-packable projects under `sources/`. Markup snippets are `.xml` files loaded by a test.

**Tech Stack:** DocFX 2.81.0 (`modern` template), GitHub Pages (`upload-pages-artifact`/`deploy-pages`), bash, xUnit, Stride 4.3, MonoGame 3.8.5.1.

**Spec:** `docs/superpowers/specs/2026-10-10-docs-site-design.md` (Plan 1 = the spec's "Plan 1" list).

## Global Constraints

- Site URL `https://icyui.dev` (apex). The `CNAME` content is exactly `icyui.dev`. English is at `/`, Russian at `/ru/`, with the same relative page paths.
- The API reference covers only `IcyUI`, `IcyUI.Design`, `IcyUI.MonoGame` and `IcyUI.Stride`. It's English only.
- Both DocFX builds run with `--warningsAsErrors`.
- Any C# or markup example longer than one line comes from a snippet file. Shell commands and one-liners may be inline.
- Russian pages with content carry `source-hash: <first 12 hex chars of the SHA-256 of the English file>`. Stubs carry `translation: stub`.
- Russian prose follows `docfx/ru/_glossary.md`. Code identifiers, XML and type names stay in English.
- Content standards (spec §7):
  - second person, present tense;
  - document shipped behaviour only, with gaps in `> [!NOTE]` boxes;
  - members are linked with `<xref:…>`, not re-documented.
- Actions are pinned to SHAs: `actions/upload-pages-artifact@fc324d3547104276b827a68afc52ff2a11cc49c9 # v5.0.0`, `actions/deploy-pages@368f82528645a54fb793d4d04e342629a3f51346 # v5.0.1`. Checkout, setup-dotnet and cache use the same pins as `ci.yml`.
- Never put `Version=` on a `PackageReference`. Central Package Management applies only under `sources/`, which is why the C# snippet projects live there (see Rulings).
- Never claim Ivan's checks: the Game Studio smoke test, the live deploy, his review of the pages.
- `dotnet test` prints "Passed!" even after a host crash, so read the `Total:` count.

## Rulings made while planning (deviations from the spec)

1. **C# snippet projects** live at `sources/IcyUI.Docs.Snippets.Stride` and `sources/IcyUI.Docs.Snippets.MonoGame`, not `docfx/snippets/Snippets.*`. Central Package Management and `Directory.Build.props` only apply under `sources/`. Markup snippets stay in `docfx/snippets/markup/`.
2. **Russian search:** the `modern` template's `lunrLanguages: ['en', 'ru']` option enables Russian stemming. The spec's "exact word forms only" limitation should disappear; Task 1 verifies this.
3. **Russian UI chrome:** a `docfx/template-ru/` folder with a Russian `token.json` translates the template's own strings ("In this article", "Search", ...) for the Russian build only. The spec didn't mention this.
4. **The language switcher** is an `iconLinks` entry computed per page in `main.js` (a documented option), not DOM manipulation.
5. **The Home hero image** is added when Ivan supplies `docfx/images/hero.png`. Plan 1 ships Home without it rather than with a placeholder.
6. **The Game Studio check is the Getting started page itself.** Ivan follows the page on a fresh Game Studio project. If it fails, the fix is a follow-up task on this plan, and the page is not published as verified until it passes.

## Review Focus

1. **An API link on a Russian page.** It must land on the English API page (`/api/...`), not on `/ru/api/...`, which is a 404. Pinned in Task 1 Step 7 (`grep` on the built Russian page).
2. **The switcher on an API page.** From `/api/...`, the RU link must go to the Russian home, not to the nonexistent `/ru/api/...`. Pinned in Task 1 Step 3 (the `main.js` logic) and Step 7 (a browser check in the preview).
3. **The site served under a path prefix** (`ioexcept10n.github.io/IcyUI/` before DNS, or the local `docfx serve`). The switcher must compute links from `docfx:rel`, not from `/`. Pinned in Task 1 Step 3; checked in Step 7.
4. **A markup snippet that references a data-context property or a resource.** It must load in the test without a running game. Pinned in Task 2 Step 2: the test supplies `DocsSnippetViewModel`, and a snippet uses `{Binding}`.
5. **A Russian page edited without updating `source-hash`,** or an English page renamed. These must surface as warnings, not silent drift. Pinned in Task 3 Step 2 (the stale and orphan fixtures).

---

### Task 1: DocFX tooling, the two-language skeleton and the build script

**Files:**
- Create:
  - tooling: `.config/dotnet-tools.json`, `.github/scripts/build-docs.sh`;
  - configs: `docfx/docfx.en.json`, `docfx/docfx.ru.json`;
  - templates: `docfx/template/public/main.js`, `docfx/template-ru/token.json`;
  - English skeleton: `docfx/en/index.md`, `docfx/en/toc.yml`, `docfx/en/CNAME`, `docfx/en/getting-started/toc.yml`;
  - Russian skeleton: `docfx/ru/index.md`, `docfx/ru/toc.yml`, `docfx/ru/getting-started/toc.yml`.
- Move:
  - `docfx/docs/markup-spec.md` → `docfx/en/markup/index.md` (rewritten in Task 5);
  - `docfx/docs/icyui-markup.xsd` and `docfx/docs/icyui-markup-x.xsd` → `docfx/en/markup/`.
- Delete: `docfx/docfx.json`, `docfx/index.md`, `docfx/toc.yml`, `docfx/docs/introduction.md`, `docfx/docs/getting-started.md`, `docfx/docs/toc.yml`.
- Modify: `.gitignore`.

**Interfaces:**
- Produces:
  - `bash .github/scripts/build-docs.sh` (run from the repo root) restores the tool, runs metadata, the English build, the xrefmap re-base and the Russian build. It exits non-zero on any DocFX warning. The output lands in `docfx/_site/`.
  - Folder conventions: English pages under `docfx/en/<section>/`, Russian under `docfx/ru/<section>/`. Every folder that holds pages has its own `toc.yml`.

- [ ] **Step 1: The failing check.**

```bash
ls .github/scripts/build-docs.sh docfx/docfx.en.json 2>&1 | head -2; ls docfx
```
Expected: "No such file" for both, and `docfx` lists `docfx.json docs index.md toc.yml`.

- [ ] **Step 2: Add the tool manifest and the configs.**

`.config/dotnet-tools.json` (repo root; `MonoGame Sample` has its own manifest, which is unaffected):
```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "docfx": {
      "version": "2.81.0",
      "commands": ["docfx"],
      "rollForward": false
    }
  }
}
```

`docfx/docfx.en.json`:
```json
{
  "metadata": [
    {
      "src": [
        {
          "src": "../sources",
          "files": [
            "IcyUI/IcyUI.csproj",
            "IcyUI.Design/IcyUI.Design.csproj",
            "IcyUI.MonoGame/IcyUI.MonoGame.csproj",
            "IcyUI.Stride/IcyUI.Stride.csproj"
          ]
        }
      ],
      "dest": "api",
      "properties": { "Configuration": "Release" }
    }
  ],
  "build": {
    "content": [
      { "files": ["api/**.yml"] },
      { "src": "en", "files": ["**/*.md", "**/toc.yml"] }
    ],
    "resource": [
      { "files": ["images/**"] },
      { "src": "en", "files": ["CNAME", "markup/*.xsd"] }
    ],
    "output": "_site",
    "template": ["default", "modern", "template"],
    "globalMetadata": {
      "_appTitle": "IcyUI",
      "_appName": "IcyUI",
      "_lang": "en",
      "_enableSearch": true,
      "_disableContribution": true
    }
  }
}
```

`docfx/docfx.ru.json`:
```json
{
  "build": {
    "content": [
      { "src": "ru", "files": ["**/*.md", "**/toc.yml"], "exclude": ["_glossary.md"] }
    ],
    "resource": [
      { "files": ["images/**"] }
    ],
    "xref": ["obj/xrefmap.en.yml"],
    "output": "_site/ru",
    "template": ["default", "modern", "template", "template-ru"],
    "globalMetadata": {
      "_appTitle": "IcyUI",
      "_appName": "IcyUI",
      "_lang": "ru",
      "_enableSearch": true,
      "_disableContribution": true
    }
  }
}
```

- [ ] **Step 3: Write the template override `docfx/template/public/main.js`.**

```js
// IcyUI docs: English at the site root, Russian under /ru/ with the same page paths.
// The EN/RU link is computed per page from docfx:rel (the relative path to this build's root), so it also works when
// the site is served under a path prefix (github.io/IcyUI/, docfx serve).

function otherLanguageUrl() {
  const relMeta = document.querySelector('meta[name="docfx:rel"]');
  if (!relMeta) return null;
  const buildRoot = new URL(relMeta.content, window.location.href);
  const isRussian = document.documentElement.lang === 'ru';
  const siteRoot = isRussian ? new URL('../', buildRoot) : buildRoot;
  const pagePath = window.location.pathname.substring(buildRoot.pathname.length);

  if (isRussian) return new URL(pagePath, siteRoot).href;
  // The API reference exists only in English: send its pages to the Russian home.
  if (pagePath.startsWith('api/')) return new URL('ru/', siteRoot).href;
  return new URL('ru/' + pagePath, siteRoot).href;
}

const isRussian = document.documentElement.lang === 'ru';
const languageLink = otherLanguageUrl();

export default {
  lunrLanguages: ['en', 'ru'],
  iconLinks: [
    ...(languageLink ? [{ icon: 'translate', href: languageLink, title: isRussian ? 'English' : 'Русский' }] : []),
    { icon: 'github', href: 'https://github.com/IOExcept10n/IcyUI', title: 'GitHub' }
  ]
}
```

- [ ] **Step 4: Russian template strings.**

Fetch DocFX 2.81.0's token file and translate **every** value. The file replaces the default one wholesale, so a missing key would show as an empty label.
```bash
mkdir -p docfx/template-ru
curl -sSL https://raw.githubusercontent.com/dotnet/docfx/v2.81.0/templates/default/token.json -o "$CLAUDE_JOB_DIR/tmp/token.en.json"
grep -c '":' "$CLAUDE_JOB_DIR/tmp/token.en.json"
```
Expected: `71` keys. Write `docfx/template-ru/token.json` with the same 71 keys and Russian values, keeping placeholders like `{count}` and `{query}` verbatim. For example:
- `"inThisArticle": "В этой статье"`, `"search": "Поиск"`, `"searchResultsCount": "Результатов по запросу \"{query}\": {count}"`;
- `"searchNoResults": "По запросу \"{query}\" ничего не найдено"`, `"tocFilter": "Фильтр по названию"`;
- `"nextArticle": "Далее"`, `"prevArticle": "Назад"`, `"changeTheme": "Сменить тему"`;
- `"themeLight": "Светлая"`, `"themeDark": "Тёмная"`, `"themeAuto": "Авто"`, `"copy": "Копировать"`, `"note": "Примечание"`;
- `"warning": "Предупреждение"`, `"tip": "Совет"`, `"important": "Важно"`, `"caution": "Осторожно"`.

The API-related keys (`classesInSubtitle` …) are translated too, though the Russian build has no API pages. Check with:
```bash
diff <(grep -o '"[a-zA-Z]*":' "$CLAUDE_JOB_DIR/tmp/token.en.json" | sort) <(grep -o '"[a-zA-Z]*":' docfx/template-ru/token.json | sort) && echo "same keys"
```
Expected: `same keys`.

- [ ] **Step 5: The skeleton pages and the moves.**

```bash
mkdir -p docfx/en/markup docfx/en/getting-started docfx/ru/getting-started docfx/images
git mv docfx/docs/markup-spec.md docfx/en/markup/index.md
git mv docfx/docs/icyui-markup.xsd docfx/docs/icyui-markup-x.xsd docfx/en/markup/
git rm -q docfx/docfx.json docfx/index.md docfx/toc.yml docfx/docs/introduction.md docfx/docs/getting-started.md docfx/docs/toc.yml
printf 'icyui.dev' > docfx/en/CNAME
```

`docfx/en/toc.yml`:
```yaml
- name: Getting started
  href: getting-started/
- name: Markup
  href: markup/
- name: API
  href: ../api/
```
`docfx/ru/toc.yml`. The API entry is a root-absolute path to the English API, so DocFX doesn't resolve it as a file. The Russian markup entry is added in Task 5, when its page exists.
```yaml
- name: Начало работы
  href: getting-started/
- name: API (англ.)
  href: /api/Icy.html
```
If the strict build warns about `/api/Icy.html` (DocFX may still try to resolve root paths), use the full URL `https://icyui.dev/api/Icy.html` and record a ruling.

`docfx/en/getting-started/toc.yml` and `docfx/ru/getting-started/toc.yml` start empty (`[]`); Task 4 fills them.
`docfx/en/index.md` and `docfx/ru/index.md` start as one-line placeholders (`# IcyUI` plus a sentence); Task 4 writes them.
The Russian placeholder carries front matter:
```yaml
---
source-hash: 000000000000
---
```
It's corrected in Task 4.

- [ ] **Step 6: `.github/scripts/build-docs.sh` and `.gitignore`.**

```bash
#!/usr/bin/env bash
# Builds the documentation site into docfx/_site: API metadata, the English site, then the Russian site under /ru/.
# Every DocFX warning (broken link, missing xref, missing snippet) fails the build. Used locally and by docs.yml.
# Usage: bash .github/scripts/build-docs.sh   (from the repository root)
set -euo pipefail

dotnet tool restore
rm -rf docfx/_site docfx/obj/xrefmap.en.yml
dotnet docfx metadata docfx/docfx.en.json --warningsAsErrors
dotnet docfx build docfx/docfx.en.json --warningsAsErrors

# The Russian build links API uids to the English pages. Re-base the English xrefmap's relative hrefs to the site
# root, so a Russian page at /ru/... links /api/..., not /ru/api/....
mkdir -p docfx/obj
{ echo "baseUrl: /"; grep -v '^baseUrl:' docfx/_site/xrefmap.yml; } > docfx/obj/xrefmap.en.yml
dotnet docfx build docfx/docfx.ru.json --warningsAsErrors
```
Append to `.gitignore`:
```
# DocFX output and generated API metadata
docfx/_site/
docfx/api/
docfx/obj/
```
If `docfx/_site/xrefmap.yml` already starts with a `### YamlMime:XRefMap` line, keep that line first: write it, then `baseUrl: /`, then the rest without it. Check with `head -3 docfx/_site/xrefmap.yml` after the first English build, and adjust the brace group if needed.

- [ ] **Step 7: Build and verify.**

```bash
bash .github/scripts/build-docs.sh > "$CLAUDE_JOB_DIR/tmp/docs-build.log" 2>&1; echo "build -> $?"; grep -iE "warning|error" "$CLAUDE_JOB_DIR/tmp/docs-build.log" | head
ls docfx/_site/index.html docfx/_site/ru/index.html docfx/_site/CNAME docfx/_site/markup/index.html docfx/_site/markup/icyui-markup.xsd docfx/_site/api/Icy.UI.Canvas.html
ls docfx/_site/api | grep -ciE "test|sample"
grep -o 'lang="[a-z]*"' docfx/_site/ru/index.html | head -1
```
Expected:
- `build -> 0` with no warnings;
- all six files exist;
- `0` API files for tests or samples;
- `lang="ru"`.

The `markup/index.md` is still the old spec, which builds fine. If DocFX reports broken links inside it (its old relative links), fix the links in this task; Task 5 rewrites it anyway.

Then check the preview in a browser-like way:
```bash
(cd docfx && dotnet docfx serve _site --port 8090 > "$CLAUDE_JOB_DIR/tmp/serve.log" 2>&1 &) ; sleep 5
curl -s http://localhost:8090/ru/index.html | grep -o 'Начало работы' | head -1
curl -s http://localhost:8090/public/main.js | grep -c lunrLanguages
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:8090/api/Icy.UI.Canvas.html
```
Expected: `Начало работы`, `1`, and `200`. Stop the server afterwards (`taskkill //F //IM docfx.exe` or kill the `dotnet` process listening on 8090).

The switcher is client-side JS. Check its logic with Node if it's available (`node -e` with a stubbed `document`/`window`), or record a ruling that it's checked by Ivan in the browser. The cases:
- `/getting-started/stride.html` → `/ru/getting-started/stride.html`;
- `/ru/x.html` → `/x.html`;
- `/api/Icy.UI.Canvas.html` → `/ru/`;
- the prefix `/IcyUI/x.html` → `/IcyUI/ru/x.html`.

- [ ] **Step 8: Commit.**

```bash
git add .config/dotnet-tools.json .github/scripts/build-docs.sh .gitignore docfx/
git status --short | grep -v "^[AMDR]" ; git commit -m "Restructure the docs into a strict two-language DocFX site

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```
Check `git status`: `docfx/_site`, `docfx/api` and `docfx/obj` must not be staged.

---

### Task 2: Snippet projects and the markup-snippet test

**Files:**
- Create:
  - `sources/IcyUI.Docs.Snippets.Stride/IcyUI.Docs.Snippets.Stride.csproj`, `sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs`;
  - `sources/IcyUI.Docs.Snippets.MonoGame/IcyUI.Docs.Snippets.MonoGame.csproj`, `sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs`;
  - `docfx/snippets/markup/first-screen.xml`;
  - `sources/IcyUI.Tests/Docs/DocsMarkupSnippetTests.cs`, `sources/IcyUI.Tests/Docs/DocsSnippetViewModel.cs`.
- Modify:
  - `sources/IcyUI.sln` (add both projects via `dotnet sln add`);
  - `sources/IcyUI.Tests/IcyUI.Tests.csproj` (copy the markup snippets to the output).

**Interfaces:**
- Produces:
  - **C# region names** (Task 4 includes them):
    - `IcyUI.Docs.Snippets.Stride/GettingStarted.cs`: `#region startup-script`, `#region code-only`, `#region first-screen`;
    - `IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs`: `#region game`, `#region first-screen`.
  - **The view model:** `DocsSnippetViewModel` (namespace `Icy.Docs.Snippets`) with `string Title`, `ICommand PlayCommand`, `ICommand QuitCommand`. It's defined in **both** snippet projects (same code, for the docs) and in the test project (as the data context for markup snippets).
  - **The test:** `DocsMarkupSnippetTests.EveryMarkupSnippet_Loads(string file)` (xUnit `[Theory]`) loads every `docfx/snippets/markup/**/*.xml`.

- [ ] **Step 1: Write the failing test first.**

`sources/IcyUI.Tests/Docs/DocsSnippetViewModel.cs`:
```csharp
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Icy.Tests.Docs
{
    /// <summary>The data context markup snippets in docfx/snippets/markup may bind to; mirrors the snippet projects' view model.</summary>
    public sealed class DocsSnippetViewModel
    {
        public string Title { get; set; } = "My Game";

        public ICommand PlayCommand { get; } = new RelayCommand(() => { });

        public ICommand QuitCommand { get; } = new RelayCommand(() => { });
    }
}
```
`sources/IcyUI.Tests/Docs/DocsMarkupSnippetTests.cs`:
```csharp
using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Tests.Rendering;
using Icy.UI;

namespace Icy.Tests.Docs
{
    /// <summary>
    /// Loads every markup snippet the documentation includes (docfx/snippets/markup/**/*.xml, copied next to the test
    /// assembly) through the real markup loader, so a renamed control or property breaks a test instead of the docs.
    /// </summary>
    public class DocsMarkupSnippetTests
    {
        private static readonly string SnippetRoot = Path.Combine(AppContext.BaseDirectory, "DocsSnippets");

        public static TheoryData<string> Snippets()
        {
            var data = new TheoryData<string>();
            foreach (string file in Directory.EnumerateFiles(SnippetRoot, "*.xml", SearchOption.AllDirectories).Order())
                data.Add(Path.GetRelativePath(SnippetRoot, file));
            return data;
        }

        [Fact]
        public void SnippetFolder_IsNotEmpty() => Assert.NotEmpty(Snippets());

        [Theory]
        [MemberData(nameof(Snippets))]
        public void EveryMarkupSnippet_Loads(string file)
        {
            var configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
            configuration.UseDefaultTheme();
            var loader = new MarkupLoader(configuration);

            UIElement root = loader.Load(File.ReadAllText(Path.Combine(SnippetRoot, file)), file);
            root.DataContext = new DocsSnippetViewModel();

            Assert.NotNull(root);
        }
    }
}
```
Check that `FakeInputSystem`'s namespace matches the `using` lines: compare with `sources/IcyUI.Tests/Markup/MarkupEquivalenceTests.cs`'s `using` block and copy it if it differs.

Add to `IcyUI.Tests.csproj`, inside an `ItemGroup`:
```xml
    <None Include="..\..\docfx\snippets\markup\**\*.xml" LinkBase="DocsSnippets" CopyToOutputDirectory="PreserveNewest" />
```

- [ ] **Step 2: Run it. It fails because the folder is empty.**

```bash
dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~DocsMarkupSnippetTests" 2>&1 | tail -3
```
Expected: a failure. `SnippetFolder_IsNotEmpty` fails, or the theory errors on a missing directory.

Then add `docfx/snippets/markup/first-screen.xml`. This is "Your first screen": a centered title and two buttons, bound to `DocsSnippetViewModel`:
```xml
<!-- Data context: DocsSnippetViewModel (Title, PlayCommand, QuitCommand). -->
<StackPanel HorizontalAlignment="Center" VerticalAlignment="Center" Orientation="Vertical">
  <TextBlock FontSize="32" Margin="0,0,0,24" HorizontalAlignment="Center" Text="{Binding Path=Title}" />
  <Button Padding="24,8" Margin="0,0,0,8" Command="{Binding Path=PlayCommand}">Play</Button>
  <Button Padding="24,8" Command="{Binding Path=QuitCommand}">Quit</Button>
</StackPanel>
```
Run again. Expected: PASS for `first-screen.xml` and `SnippetFolder_IsNotEmpty`.

**Prove that the test catches drift:** temporarily change `Padding` to `Paddding` in the snippet, run (expected FAIL with a markup error naming the attribute), then revert and run again (PASS).

- [ ] **Step 3: The snippet projects.**

`sources/IcyUI.Docs.Snippets.Stride/IcyUI.Docs.Snippets.Stride.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- C# examples included by the documentation (docfx/). Compiled by CI so the docs can't drift from the API;
       never run, never packed (not an IcyShippedLibrary). -->
  <PropertyGroup>
    <RootNamespace>Icy.Docs.Snippets</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Stride.Engine" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\IcyUI.Stride\IcyUI.Stride.csproj" />
  </ItemGroup>
</Project>
```
`sources/IcyUI.Docs.Snippets.MonoGame/IcyUI.Docs.Snippets.MonoGame.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- MonoGame C# examples included by the documentation. Compile-only against DesktopGL, like IcyUI.MonoGame. -->
  <PropertyGroup>
    <RootNamespace>Icy.Docs.Snippets</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="MonoGame.Framework.DesktopGL" PrivateAssets="all" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\IcyUI.MonoGame\IcyUI.MonoGame.csproj" />
  </ItemGroup>
</Project>
```

`sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs`. Region markers wrap exactly what the page shows, and the `using` lines live inside each region so a reader can copy a whole region:
```csharp
// Examples for docfx/en/getting-started/stride.md and first-screen.md. Each #region is one example on the page.
#pragma warning disable SA1402, SA1649 // Several small example types in one file, on purpose.

namespace Icy.Docs.Snippets.Stride.StartupScriptExample
{
    #region startup-script
    using Icy.Configuration;
    using Icy.Stride.Configuration;
    using Icy.UI;
    using Icy.UI.Controls;
    using Stride.Engine;

    /// <summary>Add this script to any entity in your startup scene: it sets up IcyUI and shows a button.</summary>
    public class IcyUIStartup : StartupScript
    {
        public override void Start()
        {
            var game = (Game)Game;
            IcyUISceneRenderer overlay = game.UseIcyUI();
            IcyConfiguration configuration = game.GetIcyConfiguration();
            configuration.UseDefaultTheme();

            var canvas = new Canvas(configuration) { IsInputEnabled = true };
            overlay.Canvases.Add(canvas);

            canvas.Add(new Button { Content = new TextBlock { Text = "Hello, IcyUI" } });
        }
    }
    #endregion
}

namespace Icy.Docs.Snippets.Stride.CodeOnlyExample
{
    #region code-only
    using Icy.Configuration;
    using Icy.Stride.Configuration;
    using Icy.UI;
    using Stride.Engine;

    /// <summary>A code-only game (no Game Studio project): set up IcyUI once the scene system exists.</summary>
    public class MyGame : Game
    {
        protected override void BeginRun()
        {
            base.BeginRun();
            // Your graphics compositor and scene setup go here, before UseIcyUI().
            IcyUISceneRenderer overlay = this.UseIcyUI();
            IcyConfiguration configuration = this.GetIcyConfiguration();
            configuration.UseDefaultTheme();
            overlay.Canvases.Add(new Canvas(configuration) { IsInputEnabled = true });
        }
    }
    #endregion
}

namespace Icy.Docs.Snippets.Stride.FirstScreenExample
{
    #region first-screen
    using System.IO;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.Input;
    using Icy.Configuration;
    using Icy.Markup;
    using Icy.UI;

    /// <summary>The menu's data: what the markup's {Binding} expressions read.</summary>
    public sealed class DocsSnippetViewModel
    {
        public string Title { get; set; } = "My Game";

        public ICommand PlayCommand { get; } = new RelayCommand(() => { /* start the game */ });

        public ICommand QuitCommand { get; } = new RelayCommand(() => { /* exit */ });
    }

    public static class MainMenu
    {
        /// <summary>Loads Menu.xml (copied to the output folder) and shows it on the canvas.</summary>
        public static void Show(Canvas canvas, IcyConfiguration configuration)
        {
            UIElement menu = new MarkupLoader(configuration).Load(File.ReadAllText("Menu.xml"), "Menu.xml");
            menu.DataContext = new DocsSnippetViewModel();
            canvas.Add(menu);
        }
    }
    #endregion
}
```
`sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs`:
```csharp
// Examples for docfx/en/getting-started/monogame.md and first-screen.md. Each #region is one example on the page.
#pragma warning disable SA1402, SA1649

namespace Icy.Docs.Snippets.MonoGame.GameExample
{
    #region game
    using Icy.Configuration;
    using Icy.MonoGame.Configuration;
    using Icy.UI;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private Canvas canvas = null!;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.UseIcyUI();
            IcyConfiguration configuration = this.GetIcyConfiguration();
            configuration.UseDefaultTheme();
            canvas = new Canvas(configuration) { IsInputEnabled = true };
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
            canvas.Render();
        }
    }
    #endregion
}

namespace Icy.Docs.Snippets.MonoGame.FirstScreenExample
{
    #region first-screen
    using System.IO;
    using Icy.Configuration;
    using Icy.Markup;
    using Icy.UI;

    public static class MainMenu
    {
        /// <summary>Loads Menu.xml (copied to the output folder) and shows it on the canvas.</summary>
        public static void Show(Canvas canvas, IcyConfiguration configuration, object viewModel)
        {
            UIElement menu = new MarkupLoader(configuration).Load(File.ReadAllText("Menu.xml"), "Menu.xml");
            menu.DataContext = viewModel;
            canvas.Add(menu);
        }
    }
    #endregion
}
```
**Before writing the pages,** check every member against the code:
- `Canvas.Add`, `Canvas.Render`, `Button.Content` (`UIElement?`), `TextBlock.Text`;
- `MarkupLoader(IcyConfiguration).Load(string, string)`, `UseDefaultTheme`, `GetIcyConfiguration`.

Also check the MonoGame sample's `Initialize`/`Draw` order (`sources/MonoGame Sample/SampleGame.cs`): copy where it calls `UseIcyUI` relative to `base.Initialize()`, and whether it calls `canvas.Render()` inside or after `base.Draw`. Mirror the sample exactly; it's the verified path.

- [ ] **Step 4: Add both projects to the solution and build.**

```bash
dotnet sln sources/IcyUI.sln add sources/IcyUI.Docs.Snippets.Stride/IcyUI.Docs.Snippets.Stride.csproj sources/IcyUI.Docs.Snippets.MonoGame/IcyUI.Docs.Snippets.MonoGame.csproj
dotnet build sources/IcyUI.sln -c Release 2>&1 | grep -E "Warn|Error\(s\)|error" | tail -4
dotnet pack sources/IcyUI.sln -c Release -o "$CLAUDE_JOB_DIR/tmp/pack-check" > /dev/null 2>&1; ls "$CLAUDE_JOB_DIR/tmp/pack-check" | grep -c Snippets
```
Expected: `0 Error(s)`, and `0` snippet packages. Snippet projects aren't shipped libraries, so they get no StyleCop, and their warnings don't fail Release.

- [ ] **Step 5: Run the full suite and commit.**

```bash
dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj 2>&1 | tail -1
git add sources/IcyUI.Docs.Snippets.Stride sources/IcyUI.Docs.Snippets.MonoGame sources/IcyUI.sln sources/IcyUI.Tests docfx/snippets
git commit -m "Compile the docs' C# examples and load their markup examples in tests

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```
Expected: `Total:` is the previous count + 2 (the Fact plus one theory case), with 0 failed.

---

### Task 3: `docs.yml` and the translation check

**Files:**
- Create: `.github/scripts/check-translations.sh`, `.github/workflows/docs.yml`
- Fixtures (not committed): `$CLAUDE_JOB_DIR/tmp/tr/`

**Interfaces:**
- Consumes: `build-docs.sh` (Task 1).
- Produces:
  - `bash .github/scripts/check-translations.sh [root]`, where `root` defaults to `docfx` and holds `en/` and `ru/`.
  - It always exits 0. It prints `::warning file=<path>::` for stale pages (including the current hash to copy) and for orphan Russian pages, then a summary line: `Translations: <fresh> fresh, <stale> stale, <stub> stubs, <orphan> orphans`.

- [ ] **Step 1: Write the fixtures and the failing run.**

```bash
F="$CLAUDE_JOB_DIR/tmp/tr"; rm -rf "$F"; mkdir -p "$F/en/a" "$F/ru/a"
printf '# Fresh\n' > "$F/en/a/fresh.md"; printf '# Stale\n' > "$F/en/a/stale.md"; printf '# Stub\n' > "$F/en/a/stub.md"
h() { sha256sum "$1" | cut -c1-12; }
printf -- '---\nsource-hash: %s\n---\n# Свежая\n' "$(h "$F/en/a/fresh.md")" > "$F/ru/a/fresh.md"
printf -- '---\nsource-hash: 111111111111\n---\n# Устаревшая\n' > "$F/ru/a/stale.md"
printf -- '---\ntranslation: stub\n---\n# Заглушка\n' > "$F/ru/a/stub.md"
printf -- '---\nsource-hash: 222222222222\n---\n# Сирота\n' > "$F/ru/a/orphan.md"
printf '# glossary\n' > "$F/ru/_glossary.md"
bash .github/scripts/check-translations.sh "$F"; echo "exit $?"
```
Expected: exit `127`, because the script is missing.

- [ ] **Step 2: Write `.github/scripts/check-translations.sh`.**

```bash
#!/usr/bin/env bash
# Reports Russian doc pages whose English source changed since they were translated. Never fails the build: an English
# fix shouldn't block a deploy, but the warnings list what to re-translate. A Russian page records its source in front
# matter as `source-hash: <first 12 hex of sha256 of the English file>`; a stub records `translation: stub`.
# Usage: check-translations.sh [docs-root]   (default: docfx; expects <root>/en and <root>/ru)
set -uo pipefail

root="${1:-docfx}"
fresh=0 stale=0 stubs=0 orphans=0

front_matter_value() { # file key -> value of `key:` inside the leading --- block, or empty
  awk -v key="$2" 'NR == 1 && $0 != "---" { exit } NR > 1 && $0 == "---" { exit } NR > 1 { split($0, kv, ":"); if (kv[1] == key) { sub(/^[^:]*:[[:space:]]*/, ""); print; exit } }' "$1"
}

while IFS= read -r -d '' ru; do
  rel="${ru#"$root/ru/"}"
  [[ "$rel" == "_glossary.md" ]] && continue
  en="$root/en/$rel"
  if [[ "$(front_matter_value "$ru" translation)" == "stub" ]]; then
    stubs=$((stubs + 1))
    continue
  fi
  if [[ ! -f "$en" ]]; then
    echo "::warning file=$ru::Russian page has no English source at $en (renamed or removed?)."
    orphans=$((orphans + 1))
    continue
  fi
  current=$(sha256sum "$en" | cut -c1-12)
  recorded=$(front_matter_value "$ru" source-hash)
  if [[ "$recorded" == "$current" ]]; then
    fresh=$((fresh + 1))
  else
    echo "::warning file=$ru::Translation is stale: $en changed (recorded ${recorded:-none}, current $current). Update the text, then set source-hash: $current"
    stale=$((stale + 1))
  fi
done < <(find "$root/ru" -name '*.md' -print0 | sort -z)

echo "Translations: $fresh fresh, $stale stale, $stubs stubs, $orphans orphans"
exit 0
```
Run the fixture command again. Expected output:
- one `::warning` for `stale.md` (recorded `111111111111`, current `<hash>`);
- one `::warning` for `orphan.md`;
- the summary `Translations: 1 fresh, 1 stale, 1 stubs, 1 orphans`;
- `exit 0`.

Then run it against the real tree: `bash .github/scripts/check-translations.sh`. Expected: a warning for `ru/index.md` (placeholder hash `000000000000`) and the summary.

Then check line endings and the executable bit:
```bash
git add .github/scripts/check-translations.sh && git update-index --chmod=+x .github/scripts/check-translations.sh .github/scripts/build-docs.sh && git ls-files --eol .github/scripts/
```
Expected: `i/lf` for all four scripts.

- [ ] **Step 3: Write `.github/workflows/docs.yml`.**

```yaml
# Builds the documentation site (English + Russian) and deploys it to GitHub Pages (https://icyui.dev).
# Pull requests only build: broken links, xrefs and snippets fail; stale translations only warn.
name: Docs

on:
  push:
    branches: [platform-independent]
    paths: ['docfx/**', 'sources/IcyUI*/**', '.config/dotnet-tools.json', '.github/scripts/build-docs.sh', '.github/scripts/check-translations.sh', '.github/workflows/docs.yml']
  pull_request:
    paths: ['docfx/**', 'sources/IcyUI*/**', '.config/dotnet-tools.json', '.github/scripts/build-docs.sh', '.github/scripts/check-translations.sh', '.github/workflows/docs.yml']

concurrency:
  group: docs-pages
  cancel-in-progress: false

permissions:
  contents: read

defaults:
  run:
    shell: bash

env:
  DOTNET_NOLOGO: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true

jobs:
  build:
    name: Build the site
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1

      - uses: actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68 # v6.0.0
        with:
          global-json-file: global.json

      - uses: actions/cache@55cc8345863c7cc4c66a329aec7e433d2d1c52a9 # v6.1.0
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ runner.arch }}-${{ hashFiles('sources/Directory.Packages.props', 'sources/**/*.csproj', 'sources/MonoGame Sample/.config/dotnet-tools.json') }}
          restore-keys: nuget-${{ runner.os }}-${{ runner.arch }}-

      - name: Build the snippet projects
        run: |
          dotnet build sources/IcyUI.Docs.Snippets.Stride/IcyUI.Docs.Snippets.Stride.csproj -c Release
          dotnet build sources/IcyUI.Docs.Snippets.MonoGame/IcyUI.Docs.Snippets.MonoGame.csproj -c Release

      - name: Build the site
        run: bash .github/scripts/build-docs.sh

      - name: Check translations
        run: bash .github/scripts/check-translations.sh

      - name: Upload the site
        if: github.event_name == 'push'
        uses: actions/upload-pages-artifact@fc324d3547104276b827a68afc52ff2a11cc49c9 # v5.0.0
        with:
          path: docfx/_site

  deploy:
    name: Deploy to GitHub Pages
    needs: build
    if: github.event_name == 'push'
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    permissions:
      pages: write
      id-token: write
    steps:
      - id: deployment
        uses: actions/deploy-pages@368f82528645a54fb793d4d04e342629a3f51346 # v5.0.1
```

- [ ] **Step 4: Lint and commit.**

```bash
D="$CLAUDE_JOB_DIR/tmp/tools"; ls "$D/actionlint.exe" >/dev/null 2>&1 || echo "re-download actionlint/shellcheck as in the CI plan (Task 3 Step 1)"
"$D/actionlint.exe" -shellcheck "$D/shellcheck.exe" .github/workflows/*.yml; echo "actionlint -> $?"
"$D/shellcheck.exe" .github/scripts/*.sh; echo "shellcheck -> $?"
git add .github/workflows/docs.yml .github/scripts/
git commit -m "Build the docs site in CI, deploy it to GitHub Pages and report stale translations

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```
Expected: `actionlint -> 0` and `shellcheck -> 0`. If the tools were cleaned up, the download commands are:
- `https://github.com/rhysd/actionlint/releases/download/v1.7.12/actionlint_1.7.12_windows_amd64.zip`;
- `https://github.com/koalaman/shellcheck/releases/download/v0.11.0/shellcheck-v0.11.0.zip`.

---

### Task 4: Getting started content (English and Russian)

**Files:**
- Write:
  - `docfx/en/index.md`;
  - `docfx/en/getting-started/{installation,stride,monogame,first-screen}.md`, `docfx/en/getting-started/toc.yml`;
  - the same five under `docfx/ru/` (plus `docfx/ru/index.md`);
  - `docfx/ru/_glossary.md`.

**Interfaces:**
- Consumes: the snippet regions and `first-screen.xml` (Task 2), and the build and check scripts (Tasks 1 and 3).

This task's deliverable is prose. The plan fixes each page's outline, required facts and includes; the wording is
written at execution time, following the spec's §7 content standards.

- [ ] **Step 1: The glossary.** Write `docfx/ru/_glossary.md`: the spec §6 table plus a rule line ("code identifiers, XML, type and member names stay in English; UI labels in examples stay English"). It's excluded from the build by `docfx.ru.json`.

- [ ] **Step 2: Write the English pages to these outlines.**
  - **`index.md` (Home):**
    - one-paragraph pitch (cross-engine game UI: markup, styles, templates, binding, animations, input; Stride and MonoGame);
    - an "Install" block (inline `dotnet add package IcyUI.Stride --prerelease` / `IcyUI.MonoGame`);
    - three cards or links: Getting started, Markup reference, API;
    - a status note: alpha, APIs may change.
  - **`getting-started/installation.md`:**
    - requirements: .NET 10 SDK, Windows (x64 and ARM64), Stride 4.3+ or MonoGame 3.8.5.1+;
    - package table: IcyUI (core, pulled in automatically), IcyUI.Stride, IcyUI.MonoGame, IcyUI.Design (dev-time only);
    - `--prerelease`;
    - a MonoGame backend note: reference DesktopGL or WindowsDX yourself; an older MonoGame fails with `FileLoadException`;
    - "Next" links.
  - **`getting-started/stride.md`:**
    - **Game Studio project:** step 1, create a project in Stride Game Studio; step 2, add the package (`dotnet add <game>.csproj package IcyUI.Stride --prerelease`, or NuGet in your IDE); step 3, create the script and include `startup-script`; step 4, add the script to an entity in the startup scene (Game Studio: select the entity → Add component → the script); step 5, run (the button draws over the scene).
    - **Without Game Studio:** include `code-only`, and note that the graphics compositor must exist before `UseIcyUI()`.
    - **Notes:** the `> [!NOTE]` "IcyUI draws on top of your scene's compositor output"; "Next: Your first screen".
  - **`getting-started/monogame.md`:**
    - steps: create a MonoGame project (`dotnet new mgdesktopgl` or `mgwindowsdx`), add the package, include `game`, run;
    - a backend note (DesktopGL vs WindowsDX);
    - `> [!NOTE]` boxes: no touch input on DesktopGL, and a possible shutdown hang on Windows-on-ARM64 with DesktopGL;
    - "Next: Your first screen".
  - **`getting-started/first-screen.md`:**
    - "a main menu in markup";
    - include `../../snippets/markup/first-screen.xml` (`[!code-xml[](../../snippets/markup/first-screen.xml)]`), saved as `Menu.xml` with "Copy to output directory";
    - explain `{Binding Path=...}` and `DataContext`;
    - tabs with the C# for each engine: DocFX tabs syntax, `# [Stride](#tab/stride)` including `../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#first-screen`, and `# [MonoGame](#tab/monogame)` including the MonoGame `first-screen` region;
    - "What you built" and "Next" (the markup reference, the API).
  - **`getting-started/toc.yml`:** Installation, Stride, MonoGame, Your first screen, in that order.

  **Include path form:** `[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#startup-script)]`. Paths are relative to the page file. Check during the build that DocFX resolves includes outside `docfx/`. If it reports them, copy the snippet files into the build via a `resource`/`overwrite` mapping and record a ruling.

- [ ] **Step 3: Build in English and check the includes.**

```bash
bash .github/scripts/build-docs.sh > "$CLAUDE_JOB_DIR/tmp/docs-build.log" 2>&1; echo "build -> $?"; grep -iE "warning|error" "$CLAUDE_JOB_DIR/tmp/docs-build.log" | head
grep -c "class IcyUIStartup" docfx/_site/getting-started/stride.html
grep -c "PlayCommand" docfx/_site/getting-started/first-screen.html
```
Expected: `build -> 0`, then `1` and at least `1`.

**Prove the strict build catches a bad include:** temporarily change `#startup-script` to `#startup-scriptx`. Expected: the build fails with a warning-as-error. Then revert.

- [ ] **Step 4: Write the Russian pages.**
  - Translate all six pages (Home + five) with the glossary. The includes are the same; the code stays identical.
  - Each Russian page gets `source-hash: <sha256 of the English file, first 12 hex>`, computed **after** the English file is final: `sha256sum docfx/en/getting-started/stride.md | cut -c1-12`.
  - The Russian `getting-started/toc.yml` uses Russian names: Установка, Stride, MonoGame, Ваш первый экран.

- [ ] **Step 5: Full build, translation check, commit.**

```bash
bash .github/scripts/build-docs.sh > "$CLAUDE_JOB_DIR/tmp/docs-build.log" 2>&1; echo "build -> $?"
bash .github/scripts/check-translations.sh
git add docfx/en docfx/ru
git commit -m "Write Home and Getting started in English and Russian

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```
Expected: `build -> 0`, and `Translations: 6 fresh, 0 stale, 0 stubs, 0 orphans`.

---

### Task 5: The markup reference at `/markup` (English and Russian)

**Files:**
- Rewrite: `docfx/en/markup/index.md`
- Create: `docfx/ru/markup/index.md`, plus `- name: Разметка` / `href: markup/` in `docfx/ru/toc.yml`

**Interfaces:**
- Consumes: the XSDs moved in Task 1, which stay in English only and are linked from both pages as `/markup/icyui-markup.xsd`.

- [ ] **Step 1: Rewrite the English page from the spec into reference tone.**
  - **Keep** every section's *behaviour* (§1–§9 of the old spec): namespaces and resolution, attributes, directives, extensions, attached properties, content and property elements, `x:Class` and navigation, the error model, the schema.
  - **Remove:**
    - the status banner and every milestone note (21 `M0`–`M5` mentions);
    - the "Deviation from the original plan" note (historical; it mentions MysticUI);
    - "Open questions": move the **still open** items into one `> [!NOTE] Not yet supported` list.
  - **Additions:**
    - a top intro: "IcyUI markup is an XML dialect…" (keep the opening example);
    - a line stating that `https://icyui.dev/markup` is the namespace this page documents;
    - links to the two XSDs, described honestly as a skeleton schema (namespaces and `x:` directives, not every control).
  - **Inline examples:** short syntax fragments that show a grammar rule, not a UI, may stay inline (the spec allows one-liners; multi-line grammar fragments are reference notation). Any full, runnable example moves to `docfx/snippets/markup/` and is included, so the test covers it.
  - **Links:** link types with `<xref:…>` where the page names public types: `MarkupLoader`, `IAssemblyResolver`, `MarkupException` and so on. The strict build verifies every uid.

- [ ] **Step 2: Build, then translate.**

```bash
bash .github/scripts/build-docs.sh > "$CLAUDE_JOB_DIR/tmp/docs-build.log" 2>&1; echo "build -> $?"; grep -iE "warning|error" "$CLAUDE_JOB_DIR/tmp/docs-build.log" | head
grep -ciE "\bM[0-5]\b|milestone|MysticUI" docfx/en/markup/index.md
```
Expected: `build -> 0`, and `0`.

Then write `docfx/ru/markup/index.md`: a translation with the glossary, code identical, the `source-hash` of the final English file. Add the toc entry. Rebuild and run the check: expected `Translations: 7 fresh, 0 stale, 0 stubs, 0 orphans`.

Then check `xref` on the Russian page:
```bash
grep -o 'href="[^"]*Icy\.Markup\.MarkupLoader[^"]*"' docfx/_site/ru/markup/index.html | head -2
```
Expected: an `href` beginning `/api/`, not `api/` or `../api/` relative to `/ru/`.

- [ ] **Step 3: Commit.**

```bash
git add docfx/en/markup docfx/ru
git commit -m "Publish the markup reference at /markup in English and Russian

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```

---

### Task 6: Links, the README and CLAUDE.md

**Files:**
- Modify:
  - `sources/Directory.Build.targets` (`PackageProjectUrl`);
  - `sources/IcyUI/README.md`, `sources/IcyUI.Design/README.md`, `sources/IcyUI.MonoGame/README.md`, `sources/IcyUI.Stride/README.md`;
  - `README.md` (root);
  - `CLAUDE.md`.

- [ ] **Step 1: The failing check.**

```bash
grep -c "icyui.dev" sources/Directory.Build.targets sources/IcyUI*/README.md README.md CLAUDE.md
```
Expected: `0` everywhere.

- [ ] **Step 2: Package links.**
  - `sources/Directory.Build.targets`: replace the `PackageProjectUrl` line and its comment with
    `<PackageProjectUrl>https://icyui.dev</PackageProjectUrl>`. The comment about `main` is obsolete: the docs site
    doesn't depend on the branch.
  - In each package README, add a line above "Source and samples": `Documentation: https://icyui.dev`.
    - IcyUI.Stride: `https://icyui.dev/getting-started/stride.html`;
    - IcyUI.MonoGame: `https://icyui.dev/getting-started/monogame.html`;
    - IcyUI, IcyUI.Design: `https://icyui.dev`.

- [ ] **Step 3: Refresh the root `README.md`:**
  - **Status:** "Early alpha (0.1.0-alpha). Published on NuGet as prerelease packages; APIs may change between alphas." Plus links: Documentation `https://icyui.dev` (Russian: `https://icyui.dev/ru/`).
  - **Engines table:**
    - `IcyUI.Stride`: integration for Stride 4.3+;
    - `IcyUI.MonoGame`: backend-neutral, DesktopGL or WindowsDX 3.8.5.1+;
    - `IcyUI.FNA`: planned.
  - **Features:** keep the current list, and add spatial navigation (keyboard/gamepad), touch gestures, DPI scaling, TreeView, and the in-game editor (`IcyUI.Design`).
  - **Building and History:** keep both sections.

- [ ] **Step 4: Add a "Documentation" subsection to `CLAUDE.md`,** after the Release subsection:
```markdown
### Documentation

The site at https://icyui.dev is built from `docfx/` by `.github/workflows/docs.yml` (deploys on pushes to `platform-independent`).

- English pages live in `docfx/en/`, Russian ones in `docfx/ru/` with the same relative paths. Every new page ships in both languages; Russian follows `docfx/ru/_glossary.md`.
- A Russian page records `source-hash:` (first 12 hex of the English file's SHA-256). `.github/scripts/check-translations.sh` warns when it's stale.
- Code examples longer than a line come from snippet files: C# from `#region`s in `sources/IcyUI.Docs.Snippets.Stride`/`.MonoGame` (compiled by CI), markup from `docfx/snippets/markup/*.xml` (loaded by `DocsMarkupSnippetTests`).
- Build and preview locally: `bash .github/scripts/build-docs.sh`, then `dotnet docfx serve docfx/_site`. The build is strict: any DocFX warning fails it.
- The API reference covers the four shipped libraries; their XML docs are its only source.

One-time hosting setup (done 2026-10-11): `icyui.dev` at Cloudflare (DNS only) → GitHub Pages (source "GitHub Actions", custom domain `icyui.dev`, HTTPS enforced), domain verified on the GitHub profile.
```
In CLAUDE.md's Layout table, the `Shared Samples` row is unchanged. Add a row:
`| IcyUI.Docs.Snippets.Stride / .MonoGame | Docs examples | Compiled C# examples the docs include; never packed or run. |`

- [ ] **Step 5: Verify and commit.**

```bash
grep -c "icyui.dev" sources/Directory.Build.targets sources/IcyUI*/README.md README.md CLAUDE.md
rm -rf artifacts && dotnet pack sources/IcyUI/IcyUI.csproj -c Release -o artifacts > /dev/null 2>&1; unzip -p artifacts/IcyUI.0.1.0-alpha.1.nupkg IcyUI.nuspec | grep projectUrl
dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj 2>&1 | tail -1
git add sources/Directory.Build.targets sources/IcyUI*/README.md README.md CLAUDE.md
git commit -m "Point the packages and the README at icyui.dev, and document the docs workflow

Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
```
Expected:
- at least 1 `icyui.dev` per file;
- `<projectUrl>https://icyui.dev/</projectUrl>` (NuGet may add a trailing slash);
- the test `Total:` from Task 2, with 0 failed.

- [ ] **Step 6: Hand over to Ivan (nothing claimed).**
  1. Push. Then check that the **Docs** workflow is green and the site is live at `https://icyui.dev` and `https://icyui.dev/ru/`. In repo **Settings → Pages**, enter `icyui.dev` if it isn't set yet, and tick **Enforce HTTPS** once it's available.
  2. **Game Studio check:** follow `https://icyui.dev/getting-started/stride.html` on a fresh Game Studio project, installing `IcyUI.Stride` from CI's `packages` artifact through a local feed folder. If the button doesn't appear or doesn't respond, report it; the fix becomes a follow-up task before alpha.1.
  3. **Review:** all seven pages in English and Russian, and the glossary terms.
  4. **Optional:** a hero screenshot as `docfx/images/hero.png` for Home.
  5. **Then the first release:** bump nothing (it's still `0.1.0-alpha.1`), tag `v0.1.0-alpha.1`, and approve.
