# Contributing to IcyUI

Thanks for helping with IcyUI. The project is in early alpha (`0.1.0-alpha`), so APIs can still change between alphas. Bug reports, documentation fixes and feedback are all useful.

## Before you start

- **Bugs:** search the [existing issues](https://github.com/IOExcept10n/IcyUI/issues) first, then use the bug report form.
- **Features and API changes:** open a feature request and discuss it before writing code. Changes to public API, rendering, input or an engine integration need agreement first, so the work doesn't go to waste.
- **Small fixes** (typos, a clear bug with an obvious fix, docs wording): a pull request on its own is fine.

Design notes for the maintainer live in a private repository. You don't need access to it: put the design in the issue or pull request instead.

## Branches

The project uses git-flow:

| Branch | Purpose | Pull request target |
|---|---|---|
| `develop` | Integration branch for in-progress work | Base for all feature work |
| `feature/<topic>` | One feature or fix, branched from `develop` | `develop` |
| `hotfix/<topic>` | Urgent fix to the latest release, branched from `main` | `main` |

`main` holds the latest release, and `release/<x.y.z>` branches are maintainer-only. Fork the repository, branch from `develop`, and open the pull request against `develop`.

## Development setup

- Install the **.NET 10 SDK**. `global.json` pins the version.
- Build and test on **Windows**. CI runs only on Windows x64 and Windows ARM64, so other platforms are untested.

```
dotnet build "sources/IcyUI.sln"
dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj"
```

The `MonoGame Sample` folder has a space in its path, so always quote it. The first build restores `dotnet-mgcb` from a local tool manifest.

## Code guidelines

- **Zero warnings.** The shipped libraries (`IcyUI`, `IcyUI.Design`, `IcyUI.MonoGame`, `IcyUI.Stride`) build with no warnings in Debug or Release. StyleCop is enabled for them, and Release turns warnings into errors.
- **Documentation.** Every public API needs complete XML documentation. Use `<see cref>`, `<see langword>` and `<list>` where they help. The API reference on [icyui.dev](https://icyui.dev) is generated from these comments.
- **Engine boundaries.** `IcyUI` (core) must not reference engine types. Engine code belongs in `IcyUI.<Engine>`. A change to rendering, input or public API must be checked against both MonoGame and Stride.
- **Layout limits.** `Width`, `MinWidth`, `MaxWidth` and similar properties use `float.NaN` for "unset". Don't pass them straight to `float.Clamp`, `Math.Min` or `Math.Max`. Use the existing `ClampToLimits`-style helpers.
- **Logging.** Use the `[LoggerMessage]` methods in `Diagnostics/Logging/IcyLog.cs`. Log once per cause, never per frame.
- **Packages.** Put all package versions in `sources/Directory.Packages.props`. Never add `Version=` to a `PackageReference`.
- **Style.** Use block-scoped `namespace X { }` declarations, the file copyright header, and the naming of the code around your change.
- **Themes and controls.** A new control needs an entry in `sources/IcyUI/Resources/Themes/DefaultTheme.xml`.
- **Demos.** Register a new demo in `sources/Shared Samples/SampleCatalog.cs` and link it into both sample hosts. `SampleCatalogTests` pins the list.
- **Tests.** Add or update tests for every behavior change. For pooled item containers (`ItemsControl` descendants), test re-targeting and re-binding behaviorally: stale content is a recurring bug there.

## Documentation

The site at [icyui.dev](https://icyui.dev) is built from `docfx/`.

- English pages live in `docfx/en/`. Russian pages live in `docfx/ru/` at the same relative path and follow `docfx/ru/_glossary.md`. A new page ships in both languages. If you can't translate it, add a stub with `translation: stub` front matter that links to the English page.
- Code examples longer than a line come from snippet files, not inline code: C# from `#region`s in `sources/IcyUI.Docs.Snippets.*` (compiled by CI) and markup from `docfx/snippets/markup/`.
- Preview the site with `bash .github/scripts/build-docs.sh`, then `dotnet docfx serve docfx/_site`. The build is strict: any DocFX warning fails it.
- Control screenshots come from `dotnet run --project sources/IcyUI.Docs.Capture`. It needs Windows and a GPU.

## Pull requests

1. Keep a pull request to one topic. Split unrelated changes into separate pull requests.
2. Fill in the pull request template, including the engines affected and the manual testing you did.
3. Make sure CI passes on both Windows runners. CI runs on every pull request.
4. Write commit messages as a short imperative summary line. Add a body when the reason isn't obvious from the diff.

A maintainer reviews each pull request as a whole before merging.

## Licensing

IcyUI is released under the [MIT license](LICENSE). By submitting a contribution, you agree that it is licensed under the same terms. No separate contributor agreement is required.

## Questions

Open an issue if you're unsure about anything. A question that needs a longer discussion is fine as an issue too.
