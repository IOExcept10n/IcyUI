# CLAUDE.md

IcyUI is a cross-engine game UI library: an engine-agnostic core plus thin per-engine integration projects. It is a solo pet project by Ivan Olyanishin (MIT), still pre-MVP.

## Branches

Git-flow, adopted 2026-10-13 after `platform-independent` merged into `main`:

- `main` is the stable branch with the code of the latest release. Only release and hotfix branches merge into it.
- `develop` is the integration branch for in-progress work, including work that isn't published yet. Base new work on it.
- `feature/<topic>` branches come off `develop` and return through a PR. CI checks them through the PR only, not on push.
- `release/<x.y.z>` branches (no `v`, no pre-release suffix) stabilize a release and carry its candidates (`v0.2.0-rc.1`, `-rc.2`, ...). They merge into `main` and back into `develop`; the stable tag goes on `main`.
- `hotfix/<topic>` branches come off `main` and merge into both `main` and `develop`.

## Layout (`sources/`)

| Project | Status | Notes |
|---|---|---|
| `IcyUI` | Core | RootNamespace `Icy`. Controls, layout, markup, styles/templates, binding, animations, input routing, rendering abstractions. Must not reference any engine types. |
| `IcyUI.Design` | Dev-time tooling | Markup design document, edit engine, editor frame, panels, overlay and workspace (Phase 10). References only `IcyUI`; games never need it at runtime. |
| `IcyUI.MonoGame` | Primary integration | Backend-neutral: compiles against DesktopGL privately; hosts reference DesktopGL or WindowsDX 3.8.5.1+. `UseIcyUI()` returns `MonoGameIcyHost` (an `IIcyHost`), a `DrawableGameComponent` that draws its `Canvases`. |
| `IcyUI.Stride` | Real integration | References `Stride.Core`/`Stride.Engine`. `UseIcyUI()` on `IGame` returns `StrideIcyHost` (an `IIcyHost`), a game system; `IcyUISceneRenderer` in the graphics compositor does the drawing. |
| `IcyUI.FNA` | Stub | Only the SDK template `Class1.cs`. Treat FNA work as greenfield, modelled on `IcyUI.MonoGame`. |
| `IcyUI.Docs.Snippets.Stride`, `.MonoGame` | Docs examples | Compiled C# examples the docs include; never packed or run. |
| `IcyUI.Docs.Capture` | Docs tool | Code-only Stride app: renders `docfx/snippets/markup/controls/*.xml` and saves the control screenshots. Never packed. |
| `IcyUI.Tests` | xUnit | Links some `Shared Samples` demos so their markup is exercised by tests. |
| `Shared Samples` | Demo source | `*Demo.cs` files linked into both sample hosts. |
| `MonoGame Sample`, `Stride Sample` | Sample hosts | Every new demo is registered in `Shared Samples/SampleCatalog.cs` and linked (`<Compile Include>`) into **both** hosts' `.csproj`; `SampleCatalogTests` pins the list. `MonoGame Sample` runs on WindowsDX by default; `-p:MonoGameBackend=DesktopGL` switches it (each backend has its own `bin/<backend>`, `obj/<backend>`). |

The built-in theme lives in `sources/IcyUI/Resources/Themes/DefaultTheme.xml` (embedded resource). New controls get an entry there.

## Build & test

Everything targets `net10.0` (the sample hosts use `net10.0-windows`). The SDK is pinned via the root `global.json` (10.0.x, `latestFeature`). Shared properties live in `sources/Directory.Build.props` (`IcyTargetFramework`, `Nullable`, `ImplicitUsings`), and **all package versions** in `sources/Directory.Packages.props` (Central Package Management). Never put `Version=` on a `PackageReference`.

CI (`.github/workflows/ci.yml`) builds and tests on x64 and ARM64 Windows, and packs on x64, for every push to `main`, `develop` and `release/**`, and every PR. The only scripts are the two checks in `.github/scripts/` (test results, release tag). Locally, use plain `dotnet` commands:

```
dotnet build "sources/IcyUI.sln"
dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj"
```

- `MonoGame Sample` has a space in its path, so always quote it. It restores `dotnet-mgcb` via a local tool manifest on first build.
- StyleCop is wired into the shipped libraries (`IcyUI`, `IcyUI.Design`, `IcyUI.MonoGame`, `IcyUI.Stride`), not into tests/samples. They build with **zero** warnings: each sets `IcyShippedLibrary`, and `sources/Directory.Build.targets` makes warnings errors in Release (NuGet audit warnings excepted). Keep Debug at zero too.
- `MonoGame Sample` opts out of `ImplicitUsings`/`Nullable`: with `UseWindowsForms`, implicit usings pull in `System.Windows.Forms`/`System.Drawing`, which clash with Icy/XNA type names.
- The MonoGame packages and the `mgcb` tools in `MonoGame Sample/.config/dotnet-tools.json` must stay on the same version.
- Development happens on both x64 and Windows-on-ARM64 machines. Engine package versions must provide `win-arm64` natives (Stride >= 4.4; MonoGame DesktopGL/WindowsDX 3.8.5 do). Missing natives fail only at runtime, not at build time.

### Release

The four shipped libraries are packages; everything else is `IsPackable=false` (`sources/Directory.Build.targets`). The version lives there too: one `<Version>` for all four, `0.1.0-alpha.N`. nuget.org never accepts the same version twice.

Releases go through `.github/workflows/release.yml` with nuget.org trusted publishing: no API key exists anywhere.

1. Bump `<Version>` in `sources/Directory.Build.targets` on the release branch (or `main` after the merge), commit, push, and wait for green CI.
2. `git tag v<version>` and `git push origin v<version>`.
3. The workflow's build job checks that the tag matches `<Version>` and where the commit is (`.github/scripts/check-release-tag.sh`): a stable version must be on `origin/main`, a pre-release on `origin/main` or its own `origin/release/<x.y.z>`. Then builds, tests and packs. Once it's green, approve the `nuget` deployment in the Actions run: the publish job pushes the packages (`.snupkg` symbols included), and a last job creates a GitHub Release with the packages attached. The Docs workflow builds the site from the same tag and waits for approval of the `github-pages` deployment: approve it after the packages are published, or reject it with a rejected release.

Every release has a built-in dry run: the publish job waits for approval, so download the build job's `packages` artifact and check it first; rejecting the deployment publishes nothing. The same goes for the docs: reject the `github-pages` deployment too. CI's `packages` artifact is the same build for any pushed commit. A manual run of `release.yml` never publishes either, and GitHub offers manual runs only for workflows on the default branch (`main`). To try packages locally:

```
rm -rf artifacts
dotnet pack "sources/IcyUI.sln" -c Release -o artifacts
```

Release builds set `ContinuousIntegrationBuild` (`sources/Directory.Build.props`), so PDB paths are `/_/...` and SourceLink maps them to the commit on GitHub. `PackageProjectUrl` is `https://icyui.dev`. The package READMEs' "Source and samples" links point at the repository root.

#### One-time publishing setup (Ivan)

1. **nuget.org → your profile → Trusted Publishing → new policy:**
   - repository owner `IOExcept10n`, repository `IcyUI`;
   - workflow file `release.yml`, environment `nuget`;
   - scopes: push new packages and push new versions, package glob `IcyUI*`.
2. **GitHub → IcyUI → Settings → Environments → New environment `nuget`:**
   - required reviewer: yourself;
   - deployment branches and tags: "Selected branches and tags", add the tag rule `v*`;
   - environment secret `NUGET_USER`: your nuget.org profile name (not your email).
3. **First release:**
   1. Push `main` and check that CI is green on both runners.
   2. Push the tag `v0.1.0-alpha.1`.
   3. When the run waits for review, download the build job's `packages` artifact and check it holds 4 `.nupkg` and 4 `.snupkg` files at `0.1.0-alpha.1`. Then approve the deployment, or reject it to publish nothing.

### Documentation

The site at https://icyui.dev is built from `docfx/` by `.github/workflows/docs.yml`, which builds on pushes to `main`/`develop`/`release/**`, PRs and manual runs, and deploys only when a `v*` tag is pushed, so the site always shows the last release.

- English pages live in `docfx/en/`, Russian ones in `docfx/ru/` with the same relative paths. Every new page ships in both languages; Russian follows `docfx/ru/_glossary.md`. A page that isn't translated yet gets a Russian stub (`translation: stub` front matter) linking to the English page.
- A Russian page records `source-hash:`, the first 12 hex of the English file's SHA-256 with CRs removed: `tr -d '\r' < docfx/en/<page>.md | sha256sum | cut -c1-12`. `.github/scripts/check-translations.sh` warns when it's stale.
- Code examples longer than a line come from snippet files: C# from `#region`s in `sources/IcyUI.Docs.Snippets.Stride`/`.MonoGame` (compiled by CI; namespaces `MyGame.*`, since an `Icy.*` namespace shadows `Stride`), markup from `docfx/snippets/markup/*.xml` (loaded by `DocsMarkupSnippetTests`). `build-docs.sh` checks that every include's file and `#region` exist: DocFX itself silently includes the whole file for an unknown region.
- Build and preview locally: `bash .github/scripts/build-docs.sh`, then `dotnet docfx serve docfx/_site`. Preview over HTTP: a page opened from disk has no navbar, sidebar or search. The build is strict: any DocFX warning fails it, except metadata warnings inside source-generated `*.g.cs` files.
- Control screenshots: `dotnet run --project sources/IcyUI.Docs.Capture` (Windows, needs a GPU) renders every `docfx/snippets/markup/controls/*.xml` and writes `docfx/images/controls/`. `build-docs.sh` warns when a controls snippet changed since its screenshot (by content hash, in `captured.txt`). The same run renders `docfx/snippets/markup/readme/*.xml` (unpadded) into `docs/images/`: that's where the README cover (`cover.png`) comes from, and `build-docs.sh` doesn't check its freshness.
- The API reference covers the four shipped libraries; their XML docs are its only source. `docfx/en/api/index.md` is its landing page. Russian pages link the English API with absolute `https://icyui.dev/api/...` URLs.

One-time hosting setup (done 2026-10-11): `icyui.dev` at Cloudflare (DNS only) → GitHub Pages (source "GitHub Actions", custom domain `icyui.dev`, HTTPS enforced), domain verified on the GitHub profile. Still to do for the first docs deploy: in **Settings → Environments → github-pages**, add the deployment tag rule `v*` (pushes to branches only build) and yourself as a required reviewer, so the site is published only for a release you approved.

## Conventions

- **Every public API gets complete XML documentation.** The DocFX site (`docfx/`) is generated from it. Use `<see cref>`/`<see langword>`, `<list>`, `<para>`, etc.
- Keep engine-specific code inside its `IcyUI.<Engine>` project. Core changes to rendering, input or public API must be checked against both MonoGame and Stride.
- Match surrounding code style: StyleCop rules, block-scoped `namespace X { }` declarations, file copyright header, existing naming.
- Layout limits (`Width`, `MinWidth`, `MaxWidth`, ...) use `float.NaN` as "unset". Don't pass them to `float.Clamp`/`Math.Min`/`Math.Max` directly: since .NET 9, `float.Clamp` propagates `NaN` bounds. Use `UIElement.ClampToLimits`-style handling.
- IcyUI logs through `IcyConfiguration.Logging` (`Microsoft.Extensions.Logging.Abstractions`). New messages go in `Diagnostics/Logging/IcyLog.cs` as `[LoggerMessage]` methods (ids: 1xxx fonts, 2xxx bindings, 3xxx types); log once per cause, never per frame.

## Process

1. Every feature or phase starts with a design discussion. Ivan prefers heavy brainstorming before any code.
2. Commit a spec to the private design repo (`IcyUI-design`, a sibling checkout): `specs/YYYY-MM-DD-<topic>-design.md`.
3. Commit a plan to the same repo: `plans/YYYY-MM-DD-<topic>-plan.md`.
4. Implement, then do a whole-branch review before calling a phase done.
5. Ivan runs manual smoke tests of the sample apps himself. Never claim a smoke test that didn't happen.
6. Pooled item containers (`ItemsControl` descendants) are a recurring source of stale-content bugs. Test re-targeting and re-binding behaviorally.

## Known issues

- **High priority, scheduled for the graphics-API discussion right after Tier-2:** on Windows-on-ARM64, MonoGame **DesktopGL** deadlocks on shutdown. `Game.Dispose()` → `SdlGameWindow.Dispose` never returns, because OpenGL there runs through Microsoft's `OpenGLOn12` layer (Snapdragon has no native GL driver). The window closes but the process stays alive. Stride (D3D11) is unaffected. WindowsDX has no SDL layer; whether the WindowsDX sample exits cleanly on WoA is for Ivan's smoke test.
- UI scaling (`Canvas.EffectiveScale`, `IcyConfiguration.Scaling`) follows the host's DPI awareness: a host without a PerMonitorV2 `app.manifest` reports `DisplayScale = 1` and is bitmap-stretched by Windows. Both sample hosts ship the manifest.
- **No touch on MonoGame desktop; decide during the MonoGame backend-consistency pass:**
  - The core gesture recognizer works, but MonoGame DesktopGL never fills `TouchPanel`. Its SDL layer declares the finger event types but never reads finger data.
  - SDL2 itself tracks fingers, so polling `SDL_GetTouchFinger` from the connector is a known way out (same `SDL2.dll` as `MonoGameNativeWindow`).
  - `IcyUI.MonoGame` is backend-neutral and `MonoGame Sample` defaults to WindowsDX (`-p:MonoGameBackend=DesktopGL` for the other). The touch gap is DesktopGL's; WindowsDX touch is unverified.
  - An SDL touch fix would apply to DesktopGL hosts only.

## Naming history

The project was renamed MysticUI → AquaUI → IcyUI. The GitHub repository was renamed to `IOExcept10n/IcyUI` on 2026-10-10; only the local folder is still named `MysticUI`. Remaining old-name references outside historical docs are bugs, so flag them.
