# Building from source

For contributors. Get the source from [GitHub](https://github.com/IOExcept10n/IcyUI) and work on the
`platform-independent` branch, the long-lived development branch.

## Requirements

- The .NET 10 SDK. The root `global.json` pins it (10.0.x).
- Windows to build the Stride and WindowsDX samples. CI builds and tests on Windows, x64 and ARM64.

## Build and test

```
dotnet build "sources/IcyUI.sln"
dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj"
```

`dotnet test` can print "Passed!" even when the test host crashed, so check the `Total` count too. The `MonoGame Sample`
folder has a space in its name: quote it. It runs on WindowsDX; add `-p:MonoGameBackend=DesktopGL` for DesktopGL.

## Conventions

- The four shipped libraries (`IcyUI`, `IcyUI.Design`, `IcyUI.MonoGame`, `IcyUI.Stride`) build with **zero warnings**.
  StyleCop is on, and Release treats warnings as errors.
- Every public API has complete XML documentation: the API reference is generated from it.
- The core never references an engine; engine code stays in its `IcyUI.<Engine>` project, and a core change to rendering,
  input or public API is checked against both integrations.
- Package versions live in one place, `sources/Directory.Packages.props`.

## Building these docs

```
bash .github/scripts/build-docs.sh
dotnet docfx serve docfx/_site
```

Preview over HTTP: a page opened from disk has no navbar or search. The build is strict: any DocFX warning fails it. Every
page exists in English (`docfx/en`) and Russian (`docfx/ru`); a page that isn't translated yet gets a Russian stub.
Code examples come from compiled snippet projects and tested markup files, so they can't drift from the library.

## Pull requests

Open an issue first for anything larger than a fix, so the design can be discussed. A change comes with tests, and
behavior that users can see comes with documentation in both languages.
