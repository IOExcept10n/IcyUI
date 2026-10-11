# Startup experience: host object, fluent configuration, logging, default fonts

> Design spec, discussed with Ivan on 2026-10-11/12, after his review of docs Plan 1. Everything here ships **before**
> `v0.1.0-alpha.1`: nothing is published yet, so the public API changes cost nothing now and would be breaking later.
> One plan implements it.

## Context

Ivan's review of the docs site and his Game Studio check turned up two release blockers and a cluster of API problems:

- **Stride 4.4.0 shipped** (net10.0 templates, new graphics APIs). `IcyUI.Stride` is built against 4.3.0.2507, and
  Stride's assembly processor injects a module initializer that calls 4.3 APIs. In a 4.4 game the asset compiler fails
  with `TypeInitializationException: The type initializer for '<Module>' threw an exception`. The NuGet package would
  fail the same way: it allows Stride >= 4.3, so NuGet resolves 4.4.0 in a new project. A trial build of
  `IcyUI.Stride` against 4.4.0 compiles with 0 errors.
- **IcyUI has no built-in font.** `FontSystem.DefaultFontFamily` is empty, `FallbackFont` is null and system fonts
  are off until `EnableSystemFonts()`. The sample hosts import `Airfool.otf` themselves; the docs' Getting started code
  doesn't, so its buttons render no text, silently. Verified: after `EnableSystemFonts()`, `"Segoe UI"` (Regular,
  Bold) and `"Arial"` resolve by family name; indexing `C:\Windows\Fonts` takes 200-400 ms.
- **No logging exists.** Failures such as the font miss above are swallowed by `catch { }` blocks or fallbacks, with
  nothing to tell the developer why text doesn't appear.
- **Setup is boilerplate, and differs per engine.** Stride: `(Game)Game` cast, `UseIcyUI()` returns the overlay, the
  user creates a `Canvas`, adds it to `overlay.Canvases`, calls `configuration.UseDefaultTheme()`. MonoGame: the user
  keeps a canvas field and calls `canvas.Render()` in `Draw()`, although `UseIcyUI()` already adds a
  `DrawableGameComponent` (`MonoGameIcyRenderer`) that never draws.
- **The configuration builder has drifted from the configuration:**

  | Section | Builder today |
  |---|---|
  | `Fonts` (`FontSystem`) | missing: created in `IcyConfiguration`'s constructor, settable only after `Build()` |
  | `Theme` | `ConfigureTheme(Action)` exists, but `UseDefaultTheme()` needs the built configuration |
  | Markup assets | `AddMarkupSupport(builder, configuration)` needs the built configuration, so no engine calls it; `configuration.Assets` can't load `.xml` |
  | `Input` | `ConfigureInput()` only guards non-null; gesture settings reachable only through `configuration.Input.Events.Gestures.Settings` |
  | `Types` | markup options and debug tools (`ConfigureMarkup`, `ConfigureDiagnostics`) hide under it |
  | `Action` overloads | Theme, Scaling, Markup, Diagnostics have them; Assets, Types, Input, Rendering don't |
  | Logging | doesn't exist |

- **`UseIcyUI(configure)` replaces the engine defaults** instead of extending them: a user who customizes anything must
  remember `WithDefaultStrideConfiguration(game)` / `WithDefaultMonoGameConfiguration(game)`, or `Build()` fails on a
  missing input system. Calling `UseIcyUI()` twice adds a second game system and wraps Stride's compositor twice.
- **`Button.Content` is a `UIElement`**, so `new Button { Content = "Play" }` doesn't compile; markup's
  `<Button>Play</Button>` works through `UIElementTextAdapter`.

## Decisions

| # | Decision | Choice |
|---|---|---|
| 1 | Timing | Before alpha.1 (Ivan: "this should go before alpha.1") |
| 2 | Default font | System fonts + `"Segoe UI"`, Windows-first; a docs note teaches other platforms to import their own font. No bundled font. |
| 3 | Logging | `Microsoft.Extensions.Logging.Abstractions`, configured through the configuration builder |
| 4 | Builder shape | Keep the current shape (one builder, `Configure…()` sections) and fill the gaps; not a Microsoft.Extensions-style rewrite |
| 5 | What `UseIcyUI()` returns | A host object (`IIcyHost`), plus `GetIcy…()` extensions for access from other scripts |
| 6 | Stride extension target | `IGame`, not `Game` (no cast in scripts) |
| 7 | Text as content | An implicit `string` → `UIElement` conversion that creates a `TextBlock`; no WPF-style `object` content |

## Design

### 1. The host

Core (`Icy.UI` namespace, next to `Canvas`):

```csharp
public interface IIcyHost
{
    IcyConfiguration Configuration { get; }
    Canvas MainCanvas { get; }        // created by UseIcyUI, IsInputEnabled = true
    IList<Canvas> Canvases { get; }   // drawn in list order, later on top; MainCanvas is [0]
}
```

| | Stride (`IcyUI.Stride`) | MonoGame (`IcyUI.MonoGame`) |
|---|---|---|
| Host type | `StrideIcyHost : GameSystemBase, IIcyHost` (today's internal `IcyUIGameSystem`, renamed and public) | `MonoGameIcyHost : DrawableGameComponent, IIcyHost` (today's internal `MonoGameIcyRenderer`, renamed and public) |
| Extensions on | `IGame` | `Game` |
| Draws `Canvases` | `IcyUISceneRenderer` in the compositor, reading the host's list | its own `Draw` override; `DrawOrder = int.MaxValue` by default |
| Engine-specific extra | `SceneRenderer` (the `IcyUISceneRenderer`) | `DrawOrder`, `Visible` (inherited) |

Extensions, same names on both engines:

- `UseIcyUI()` / `UseIcyUI(Action<IConfigurationBuilder> configure)` returns the engine's host type. The engine
  defaults (section 2) are applied first, then `configure`, so the lambda only overrides what it touches. A second
  call throws `InvalidOperationException` ("IcyUI is already set up for this game; use GetIcyHost()").
- `GetIcyHost()`, `GetIcyConfiguration()`, `GetIcyCanvas()` (the main canvas): throw `InvalidOperationException`
  naming `UseIcyUI()` when it hasn't run. `TryGetIcyHost(out …)` for code that must check.
- `WithDefaultStrideConfiguration` / `WithDefaultMonoGameConfiguration` stay public (they are what `UseIcyUI()`
  applies), for users who build a configuration by hand.

Stride on `IGame`: the integration uses `Services`, `GameSystems`, `Content`, `GraphicsDevice` and `Window`, all on
`IGame`; `SceneSystem` comes from `game.Services.GetService<SceneSystem>()`. The internal types that take a `Game`
today (`InputSystem`, `TextureImporter`, `StrideAssetContextFactory`, …) take `IGame`. `UseIcyUI()` still requires the
scene system's graphics compositor to exist (a startup script's `Start()`, or after `BeginRun()` in code-only games)
and throws a clear `InvalidOperationException` when it doesn't.

Rules, documented in the XML docs and on the Getting started pages:

- MonoGame components draw during `base.Draw()`: anything the game draws *after* `base.Draw()` lands on top of the UI.
- Rendering a canvas yourself (`canvas.Render()`, e.g. into a render target) still works; such a canvas must not be in
  `Canvases`, or it's drawn twice.
- FNA, when implemented, mirrors the MonoGame host.

### 2. The configuration builder

Shape unchanged: `IcyConfigurationBuilder` implements every section interface; `Configure…()` switches section for
chaining. Changes:

- **Deferred steps.** New `IConfigurationBuilder.AfterBuild(Action<IcyConfiguration> step)`: steps that need the
  built configuration are recorded and run inside `Build()`, right after the configuration is constructed, in the
  order they were declared. The fonts, theme and markup-support methods below are built on it, and users can call it
  directly.
- **Both forms for every section:** `ConfigureX()` (chaining) and `ConfigureX(Action<…>)` (lambda, receives the
  section object): Assets (`AssetConfiguration`), Types (`ReflectionConfiguration`), Input (`IInputSystem`),
  Rendering (`IRenderContext`), Theme, Scaling, Fonts, Logging.
- **New sections:**
  - `ConfigureFonts()` → `IFontConfigurationBuilder`; `ConfigureFonts(Action<FontSystem>)` is deferred (section 4).
  - `ConfigureLogging()` → `ILoggingConfigurationBuilder` (section 3).
  - `ConfigureGestures(Action<GestureSettings>)` on the input section, applied to the input system's gesture settings.
  - `ConfigureDiagnostics(Action<DebugToolRegistry>)` and `ConfigureMarkup(Action<MarkupConfiguration>)` become
    extensions on `IConfigurationBuilder` (today on `IReflectionConfigurationBuilder`), so they're found without
    first calling `ConfigureTypes()`. Their data stays in `ReflectionConfiguration`.
- **Engine defaults** (`WithDefaultStrideConfiguration` / `WithDefaultMonoGameConfiguration`), in addition to what
  they register today: `AddMarkupSupport()`, `UseDefaultTheme()`, `ConfigureFonts().UseSystemFonts()
  .WithDefaultFontFamily("Segoe UI")`, and the engine's default log provider.
- `IcyConfiguration` gets `Logging` (`ILoggerFactory`); its constructor takes it as an optional parameter
  (default `NullLoggerFactory.Instance`).

Every new public member gets complete XML documentation, per the project convention.

### 3. Logging

- **Dependency:** `Microsoft.Extensions.Logging.Abstractions` in `IcyUI` (version in `Directory.Packages.props`). It
  contains the interfaces and the `[LoggerMessage]` source generator, nothing else. No integration adds a logging
  package; games that want console or Serilog output bring their own.
- **Builder** (`ILoggingConfigurationBuilder`):
  - `UseLoggerFactory(ILoggerFactory)`: replaces everything; filtering is then the factory's job. IcyUI never disposes
    a factory it didn't create.
  - `AddProvider(ILoggerProvider)`: adds a provider to IcyUI's own minimal factory (`IcyLoggerFactory`, core,
    internal), which the engines use for their defaults.
  - `WithMinimumLevel(LogLevel)`: the minimum level of IcyUI's own factory. Default `Warning`.
  - Root shortcuts: `ConfigureLogging(ILoggerFactory)`, `ConfigureLogging(LogLevel)`.
- **Engine defaults:**
  - Stride: a provider forwarding to `Stride.Core.Diagnostics.GlobalLogger.GetLogger(category)`, so IcyUI messages
    appear with Stride's own in the game console and Game Studio's output.
  - MonoGame: a provider writing through `System.Diagnostics.Debugger.Log` while `Debugger.IsLogging()`. Not
    `Debug.WriteLine`: it is `[Conditional("DEBUG")]` and compiled out of our Release package.
  - Without an engine (tests, custom hosts): `NullLoggerFactory`.
- **Categories** are full type names (`Icy.Rendering.Fonts.FontSystem`, `Icy.Markup.MarkupLoader`), so prefix filters
  like `"Icy.Markup"` work in any logging framework.
- **Messages** are `[LoggerMessage]` partial methods with event ids grouped by area, and say what to do:
  "No font for 'Segoe UI' (Regular): text using it won't render. Call EnableSystemFonts() or import a font file."
- **Events in this release:**

  | Level | Event | Once per |
  |---|---|---|
  | Warning | No font resolves for a family and style (`GetOrLoad` returns null) | (family, style) |
  | Warning | A system font import throws (today swallowed in `GetOrLoad`) | family |
  | Warning | A binding's path doesn't resolve on its source (member not found) | binding (a flag on the binding, not a set) |
  | Warning | An assembly can't be fully scanned for types (`ReflectionTypeLoadException` in `MarkupTypeResolver`; the empty `catch` in `AssemblyResolver`) | assembly |
  | Information | The gradient effect isn't supported; gradients fall back | process |
  | Debug | A file in the system fonts folder can't be parsed and is skipped | file |

- **Not logged:** conversion failures from user input in two-way bindings (shown by the validation UI), anything
  per frame.

### 4. Fonts

- **`IFontConfigurationBuilder`**, every method deferred to `Build()`:
  - `UseSystemFonts()` → `FontSystem.EnableSystemFonts()`;
  - `WithDefaultFontFamily(string)` → `FontSystem.DefaultFontFamily`;
  - `ImportFont(string path)` → `FontSystem.ImportFont(configuration.Assets.DefaultAssetContext, path)`;
  - `WithFallbackFont(Func<IcyConfiguration, IFont>)` → `FontSystem.FallbackFont`.
- **Lazy indexing.** `EnableSystemFonts()` only switches the feature on. The fonts folder is indexed the first time
  `GetOrLoad` misses both the cache and the already-imported fonts, then never again. A game whose default family is
  an imported font never indexes; a game using Segoe UI pays once, at its first text, instead of inside `UseIcyUI()`.
  `AreSystemFontsEnabled` keeps its meaning (the feature is on). Rejected: indexing on a background thread at startup
  (threading in the font system to hide a one-time startup cost).
- **Other platforms.** `SpecialFolder.Fonts` is the Windows fonts folder; on Linux and macOS .NET maps it to the
  user's font folder (`~/.fonts`, `~/Library/Fonts`), where Segoe UI isn't. The "no font" warning fires and the docs
  note (section 7) shows `ImportFont` with a font shipped in the game. Shipping copies of Windows fonts isn't allowed;
  loading the player's installed copy at runtime is fine.

### 5. Theme and markup loading

- **`UseDefaultTheme()` on the builder** (theme section), deferred; part of both engine defaults.
  `configuration.UseDefaultTheme()` stays, for switching back at runtime.
- **`AddMarkupSupport()` without parameters** on the asset section, deferred; part of both engine defaults. The
  overload taking an `IcyConfiguration` is removed.
- **`AssetConfiguration.Load<T>(string path)`** (and `LoadAsync<T>`): a relative path uses `DefaultAssetContext`
  (the app folder, `AssetContext.ApplicationContext`, on both engines today); a path with a scheme (`file:`,
  `icy-res:`, `icy-asset:`) goes through `AssetContextFactory`. Replaces
  `Assets.AssetResolver.LoadAsset<T>(Assets.DefaultAssetContext, path)`.
- **`Canvas.LoadMarkup(string path, object? dataContext = null)`**: loads a `UIElement` through `Assets.Load`, sets
  `DataContext` when given, adds it to the canvas and returns it. An instance method, so it needs no extra `using`.

### 6. Text as content

`UIElement` declares `public static implicit operator UIElement(string text) => new TextBlock { Text = text };`.

- `new Button { Content = "Play" }`, `panel.Children.Add("Score: 0")`, `canvas.Add("Loading…")` compile and add a
  `TextBlock`, matching markup, where bare text already works in any element slot.
- `TypeConversionManager` already discovers `op_Implicit`, so markup attributes (`Content="Play"`) and bindings
  (`Content="{Binding Title}"`) convert the same way. The implementation verifies both and documents them on the
  markup reference page.
- A binding that changes `Content` creates a new `TextBlock` per update; text that changes often should bind
  `TextBlock.Text`. Documented on the operator.
- Rejected: WPF-style `object Content` with content presenters and data-template lookup by type, a lot of machinery
  for "text on a button".

### 7. Stride 4.4, samples and docs

- **Stride 4.4.0:** `StrideVersion` in `Directory.Packages.props`. `IcyUI.Stride`, `IcyUI.Docs.Snippets.Stride` and
  `Stride Sample` build with zero warnings (NuGet audit warnings excepted, as today); the 4.4 packages are checked for
  `win-arm64` natives. `CLAUDE.md` and the `IcyUI.Stride` README say Stride 4.4+.
- **Samples:** both `SampleGame.cs` files move to the host API (`UseIcyUI()` with the sample's font import in the
  lambda; demos receive the host or its canvas). The MonoGame sample drops its manual `canvas.Render()`.
- **Docs snippets and pages** (English and Russian, re-hashed):
  - Stride and MonoGame Getting started use the new API; no font lines needed. A note, "Fonts on other platforms",
    shows `ConfigureFonts().ImportFont(...)` with a font file shipped in the game.
  - The Stride page loses the "retarget to .NET 10" step; a **Troubleshooting** section covers projects created
    before Stride 4.4, and Game Studio failing to start on Windows on ARM64 ("no compatible MSBuild found": use the
    code-only setup there).
  - The MonoGame page keeps its retarget step: the 3.8.5.1 templates target `net9.0`.
  - Your first screen uses `MainCanvas.LoadMarkup("Menu.xml", new MenuViewModel())`.
  - The markup reference documents text as an attribute value for `UIElement` properties.
  - Package READMEs with setup code use the new API.
- The deeper tutorial redesign (separate Game Studio / code-only pages, step-by-step lessons, per-feature markup pages)
  is out of scope here; it's the next brainstorm, together with docs Plans 2 and 3.

## Plans

One plan, `docs/superpowers/plans/2026-10-12-startup-experience-plan.md`. Suggested task order: Stride 4.4 → builder
deferred steps and sections → logging → fonts → theme/markup loading → text conversion → hosts (both engines) →
samples → docs snippets and pages → `CLAUDE.md`/READMEs. Then Ivan smoke-tests both sample hosts on both machines and
the Game Studio flow on the x64 PC with the CI `packages` artifact, and tags alpha.1.

## Testing

- **Builder:** deferred steps run in declaration order inside `Build()`; every `ConfigureX(Action)` reaches its
  section; `UseIcyUI(configure)` applies the engine defaults before `configure` (tested through the core part: a
  defaults step followed by an override).
- **Logging:** a capturing `ILoggerFactory` in the test project; each event in the table logs once and only once, and
  the "not logged" cases (user-input conversion errors) don't log. `WithMinimumLevel` filters; `UseLoggerFactory`
  bypasses IcyUI's factory.
- **Fonts:** Windows-only test (skipped elsewhere) that the engine-default font steps resolve Segoe UI; the fonts
  folder isn't indexed before the first miss (an internal counter visible to tests); `ConfigureFonts` steps apply in
  `Build()`.
- **Markup loading:** `Assets.Load` with a relative path and with a scheme; `LoadMarkup` adds the element and sets its
  `DataContext`; `AddMarkupSupport()` makes `Assets.Load<UIElement>("x.xml")` work.
- **Text conversion:** `Content = "Play"` creates a `TextBlock`; the markup attribute and a binding convert too.
- **Hosts:** engine-side behaviour (draw order, second `UseIcyUI()` throws, `GetIcy…` before setup throws) is covered
  where testable without a graphics device; the rest is Ivan's smoke test. The docs snippets compile in CI.
- Full suite green; the four shipped libraries at zero warnings in Debug and Release.

## Performance

- Logging when nothing listens: one `IsEnabled` check per call site, no formatting or allocation (`[LoggerMessage]`).
  Every event is once-only; dedupe state is a flag per binding and small sets keyed by family or assembly.
- Fonts: indexing (200-400 ms on Windows) moves from setup to the first font miss, and is skipped entirely when the
  default family is an imported font.
- MonoGame host: one loop over `Canvases` per frame, the same work the user's `canvas.Render()` did.
- Text conversion: allocates one `TextBlock` per conversion, as writing it by hand does.
- Deferred builder steps run once, at `Build()`.

## Known limitations

- The default font is Windows-only; other platforms need `ImportFont` (warned and documented).
- `ReflectionExtensions.ConverterInstance` stays a process-wide static, outside the configuration.
- Engine-side host behaviour is mostly verified by smoke tests, not unit tests (no graphics device in tests).

## Out of scope

- Bundling a font with IcyUI.
- WPF-style `object` content and data templates for content controls.
- A fully top-level app API (`new IcyGame().Run(...)` without a `Game` class).
- The Getting started tutorial redesign, docs Plans 2 and 3, library localization (alpha.2).
