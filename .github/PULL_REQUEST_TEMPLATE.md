## Summary

<!-- What does this change, and why? One or two sentences is enough. -->

## Related issue

<!-- Closes #123, or "none". Features and API changes need a discussed issue first (see CONTRIBUTING.md). -->

## Type of change

- [ ] Bug fix
- [ ] Feature
- [ ] Breaking change to public API
- [ ] Documentation only
- [ ] Refactor, build or CI

## Engines affected

- [ ] Core (`IcyUI`)
- [ ] `IcyUI.MonoGame`
- [ ] `IcyUI.Stride`
- [ ] `IcyUI.Design`
- [ ] None

## Checklist

- [ ] Shipped libraries build with zero warnings: `dotnet build "sources/IcyUI.sln" -c Release`
- [ ] Tests pass: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj"`
- [ ] Tests added or updated for the behavior change
- [ ] Public APIs have complete XML documentation
- [ ] Core changes were checked against both MonoGame and Stride (or the change doesn't touch core)
- [ ] English and Russian docs updated, or no docs needed (`bash .github/scripts/build-docs.sh` passes)
- [ ] New demos are registered in `SampleCatalog.cs` and linked into both sample hosts
- [ ] Targets `develop` (or `main` for a hotfix)

## Manual testing

<!-- Which sample or game, which engine and backend (Stride, MonoGame WindowsDX, MonoGame DesktopGL), and what you checked.
     Leave empty if you didn't run it. -->
