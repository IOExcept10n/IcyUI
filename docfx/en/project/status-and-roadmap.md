# Status and roadmap

## Status

IcyUI is in **alpha** (`0.1.0-alpha.N`). The core, the Stride and MonoGame integrations, the markup, the default theme
and the design tools work and are covered by tests, but public APIs may still change between alpha releases. There is no
stable release yet; the first one will follow once the alpha has been used in a real game.

| Part | Package | State |
|---|---|---|
| Core: controls, layout, markup, styles, binding, animation, input, localization | `IcyUI` | Alpha |
| Stride integration | `IcyUI.Stride` | Alpha |
| MonoGame integration (DesktopGL and WindowsDX) | `IcyUI.MonoGame` | Alpha |
| Design tools: hot reload and the in-game editor | `IcyUI.Design` | Alpha |
| FNA integration | none | Not started: `IcyUI.FNA` is a stub |

## Roadmap

These items are planned. They have no dates or version numbers.

- **An FNA integration.** See [Writing an engine integration](../guides/engine-integration.md) for what it involves.
- **Localization arguments and plural forms**, such as `"{0} coins"`.
- **Source generators:** compiled binding paths declared with `x:DataType` for ahead-of-time compilation, and page classes
  generated from markup for games that don't load UI dynamically or prefer AOT.
- **Rebindable keys** for keyboard and gamepad focus navigation.
- **Memory and performance work:** less allocation and memory use, through spans and pooling.
- **An SDK with an editor pack:** a UI project structure designed separately from the game and added to it as a package,
  a directory or another source.

Only decided items are listed. Anything missing here is not ruled out; it just hasn't been decided.
