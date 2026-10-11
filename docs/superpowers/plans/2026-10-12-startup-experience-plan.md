# Startup Experience Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make a new IcyUI game work in one call on both engines (`UseIcyUI()` returns a host with a ready canvas,
theme, markup loading and a default font), add logging, fill the configuration builder's gaps, and move to Stride
4.4, all before `v0.1.0-alpha.1`.

**Architecture:** Core gains `IIcyHost`, deferred builder steps (`AfterBuild`), a logging section on
`Microsoft.Extensions.Logging.Abstractions`, font/theme/markup builder steps, and a `string` → `UIElement`
conversion. Each engine implements the host on the component it already has (`MonoGameIcyHost`: the
`DrawableGameComponent`; `StrideIcyHost`: the `GameSystemBase`) and applies its defaults before the user's lambda.

**Tech Stack:** .NET 10, C#, xUnit 2.9, MonoGame 3.8.5.1, Stride 4.4.0, Microsoft.Extensions.Logging.Abstractions
10.0.12, DocFX 2.81.

**Spec:** `docs/superpowers/specs/2026-10-12-startup-experience-design.md`

## Global Constraints

- Every public API gets complete XML documentation (internal members don't need it: `documentInternalElements` is
  `false`) (`<summary>`, `<param>`, `<returns>`, `<exception>`, `<see cref>`,
  `<see langword>`, `<list>`/`<para>` where useful).
- The four shipped libraries (`IcyUI`, `IcyUI.Design`, `IcyUI.MonoGame`, `IcyUI.Stride`) build with **zero
  warnings** in Debug and Release (NuGet audit warnings excepted).
- Match the code style: file copyright header, block-scoped `namespace X { }`, StyleCop rules.
- Package versions only in `sources/Directory.Packages.props`; never `Version=` on a `PackageReference`.
- Stride version: `4.4.0`. MonoGame: `3.8.5.1` (unchanged).
- Engine-specific code stays in its `IcyUI.<Engine>` project; core never references an engine.
- Default font family: `"Segoe UI"`, with system fonts enabled, on both engines.
- Default minimum log level: `LogLevel.Warning`.
- Log categories are full type names (`ILoggerFactory.CreateLogger<T>()`).
- Docs: every changed page ships in English and Russian; Russian follows `docfx/ru/_glossary.md`; each Russian page's
  `source-hash` is `tr -d '\r' < docfx/en/<page>.md | sha256sum | cut -c1-12`.
- Docs snippet namespaces are `MyGame.*`.
- `dotnet test` prints "Passed!" even when the host crashes: always read the `Total` line.
- Never claim a smoke test that didn't happen: engine hosts are Ivan's to smoke-test.

## Review Focus

1. **Engine default then user override:** a theme or font the user sets in `UseIcyUI(ui => …)` must win over the
   engine default even though the default is a deferred step registered first. Pinned in Task 3
   (`ConfigureTheme_Action_RunsAfterEarlierDeferredTheme`) and Task 4 (`WithDefaultFontFamily_LastStepWins`).
2. **A missing font must not cost an exception per frame:** the second lookup of an unknown family must not retry
   the import. Pinned in Task 4 (`GetOrLoad_UnknownFamily_IsNotRetried`).
3. **A font imported after a miss must be found:** the miss cache must forget the family. Pinned in Task 4
   (`ImportFont_AfterMiss_ResolvesTheFamily`).
4. **Loading the same markup file twice must give two elements**, not one element added twice. Pinned in Task 3
   (`LoadMarkup_Twice_AddsTwoDistinctElements`).
5. **`null` strings:** `Content = (string?)null` must clear the content, not create an empty `TextBlock`. Pinned in
   Task 6 (`NullString_ConvertsToNull`).

---

## File structure

| File | Task | Responsibility |
|---|---|---|
| `sources/Directory.Packages.props` | 1, 2 | Stride 4.4.0; logging package version |
| `sources/IcyUI/Configuration/LoggingConfiguration.cs` | 2 | logging section options (factory, providers, level) |
| `sources/IcyUI/Configuration/ILoggingConfigurationBuilder.cs` | 2 | logging section builder interface |
| `sources/IcyUI/Diagnostics/Logging/IcyLoggerFactory.cs` | 2 | IcyUI's minimal `ILoggerFactory` (providers + minimum level) |
| `sources/IcyUI/Diagnostics/Logging/IcyLog.cs` | 2, 4, 5 | all `[LoggerMessage]` definitions |
| `sources/IcyUI/Diagnostics/Logging/DebuggerLoggerProvider.cs` | 2 | public provider writing to an attached debugger |
| `sources/IcyUI/Configuration/IConfigurationBuilder.cs` | 2, 3, 4 | `ConfigureLogging()`, `AfterBuild`, `ConfigureFonts()` |
| `sources/IcyUI/Configuration/IcyConfigurationBuilder.cs` | 2, 3, 4 | implementations |
| `sources/IcyUI/Configuration/IcyConfiguration.cs` | 2 | `Logging` property |
| `sources/IcyUI/Configuration/BuildingExtensions.cs` | 2, 3 | lambda overloads, theme/markup steps, logging shortcuts |
| `sources/IcyUI/Configuration/IFontConfigurationBuilder.cs` | 4 | fonts section interface |
| `sources/IcyUI/Configuration/FontBuildingExtensions.cs` | 4 | `UseSystemFonts`, `WithDefaultFontFamily`, `ImportFont`, `WithFallbackFont`, `ConfigureFonts(Action)` |
| `sources/IcyUI/Configuration/AssetConfiguration.cs` | 3 | `Load<T>`, `LoadAsync<T>` |
| `sources/IcyUI/UI/Canvas.cs` | 3 | `LoadMarkup` |
| `sources/IcyUI/Rendering/Fonts/FontSystem.cs` | 4 | lazy index, miss cache, `ResolveTextFont`, logs |
| `sources/IcyUI/UI/Controls/TextBlock.cs`, `TextBox.cs` | 4 | use `ResolveTextFont` |
| `sources/IcyUI/Data/Bindings/DynamicPropertyPath.cs` | 5 | missing-member warning |
| `sources/IcyUI/Markup/Extensions/BindingExtension.cs` | 5 | passes a logger to the path |
| `sources/IcyUI/Data/AssemblyResolver.cs` | 5 | per-assembly scan, warning |
| `sources/IcyUI/Data/TypeConversionManager.cs` | 6 | operator lookup on the target type |
| `sources/IcyUI/UI/UIElement.cs` | 6 | `implicit operator UIElement?(string?)` |
| `sources/IcyUI/UI/IIcyHost.cs` | 7 | the host contract |
| `sources/IcyUI.MonoGame/Configuration/MonoGameIcyHost.cs` | 7 | replaces `MonoGameIcyRenderer.cs` |
| `sources/IcyUI.MonoGame/Configuration/MonoGameBuildingExtensions.cs` | 7 | host extensions, defaults |
| `sources/IcyUI.Stride/Configuration/StrideIcyHost.cs` | 8 | replaces `IcyUIGameSystem.cs` |
| `sources/IcyUI.Stride/Configuration/StrideBuildingExtensions.cs` | 8 | `IGame` extensions, defaults |
| `sources/IcyUI.Stride/Diagnostics/StrideLoggerProvider.cs` | 8 | forwards to Stride's `GlobalLogger` |
| `sources/IcyUI.Stride/**` (`TextureImporter`, `StrideAssetContextFactory`, `StrideNativeWindow`) | 8 | `Game` → `IGame` |
| `sources/MonoGame Sample/SampleGame.cs`, `sources/Stride Sample/SampleGame.cs` | 7, 8 | host API |
| `sources/IcyUI.Docs.Snippets.*/*.cs`, `docfx/{en,ru}/**` , READMEs, `CLAUDE.md` | 9 | docs |
| Tests under `sources/IcyUI.Tests/` | 2–6 | see each task |

---

### Task 1: Stride 4.4.0

**Files:**
- Modify: `sources/Directory.Packages.props` (`<StrideVersion>`)
- Modify: `sources/IcyUI.Stride/README.md` (line 3), `README.md` (`IcyUI.Stride` row), `CLAUDE.md` (Build & test bullet about natives)

**Interfaces:** Produces: all Stride projects on 4.4.0.

- [ ] **Step 1: Bump the version**

In `sources/Directory.Packages.props` change `<StrideVersion>4.3.0.2507</StrideVersion>` to
`<StrideVersion>4.4.0</StrideVersion>`.

- [ ] **Step 2: Build every Stride project in Release**

Run:
```
dotnet build sources/IcyUI.Stride/IcyUI.Stride.csproj -c Release
dotnet build sources/IcyUI.Docs.Snippets.Stride/IcyUI.Docs.Snippets.Stride.csproj -c Release
dotnet build "sources/Stride Sample/Stride Sample.csproj" -c Release
```
Expected: each `Build succeeded`, `0 Error(s)`; the only warnings are `NU190x` audit warnings. If `Stride Sample`
fails on a changed Stride API, fix the call site to the 4.4 API and note the change for the final report.

- [ ] **Step 3: Check win-arm64 natives**

Run: `ls ~/.nuget/packages/stride.*/4.4.0/runtimes/ 2>/dev/null | sort -u`
Expected: a `win-arm64` folder is listed (next to `win-x64`). If not, stop and report it: Ivan's ARM64 laptop would
fail at runtime.

- [ ] **Step 4: Update the version mentions**

- `sources/IcyUI.Stride/README.md`: "Requires Stride 4.3 or later." → "Requires Stride 4.4 or later."
- `README.md`: "Integration for Stride 4.3+" → "Integration for Stride 4.4+".
- `CLAUDE.md`: "(Stride >= 4.3; MonoGame DesktopGL/WindowsDX 3.8.5 do)" → "(Stride >= 4.4; MonoGame
  DesktopGL/WindowsDX 3.8.5 do)".

- [ ] **Step 5: Run the full test suite**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj > /tmp/t1.log 2>&1; tail -5 /tmp/t1.log`
Expected: `Failed: 0`, `Total` = 1843.

- [ ] **Step 6: Commit**

```bash
git add sources/Directory.Packages.props sources/IcyUI.Stride/README.md README.md CLAUDE.md
git commit -m "Move to Stride 4.4.0: IcyUI built on 4.3 fails to load in a 4.4 game"
```

---

### Task 2: Logging infrastructure

**Files:**
- Modify: `sources/Directory.Packages.props`, `sources/IcyUI/IcyUI.csproj`
- Create: `sources/IcyUI/Configuration/LoggingConfiguration.cs`, `sources/IcyUI/Configuration/ILoggingConfigurationBuilder.cs`
- Create: `sources/IcyUI/Diagnostics/Logging/IcyLoggerFactory.cs`, `IcyLog.cs`, `DebuggerLoggerProvider.cs`
- Modify: `sources/IcyUI/Configuration/IConfigurationBuilder.cs`, `IcyConfigurationBuilder.cs`, `IcyConfiguration.cs`, `BuildingExtensions.cs`
- Create: `sources/IcyUI.Tests/Logging/CapturingLoggerFactory.cs`, `sources/IcyUI.Tests/Logging/LoggingConfigurationTests.cs`

**Interfaces:**
- Produces:
  - `IcyConfiguration.Logging : ILoggerFactory` (never null; `NullLoggerFactory.Instance` by default).
  - `IcyConfiguration(IInputSystem, AssetConfiguration, IRenderContext, ReflectionConfiguration, ThemeConfiguration? theme = null, ScalingConfiguration? scaling = null, ILoggerFactory? logging = null)`.
  - `IConfigurationBuilder.ConfigureLogging() : ILoggingConfigurationBuilder`; `ILoggingConfigurationBuilder.Logging : LoggingConfiguration`.
  - `LoggingConfiguration { ILoggerFactory? Factory; LogLevel MinimumLevel = Warning; IList<ILoggerProvider> Providers }`.
  - Extensions (in `BuildingExtensions`): `ConfigureLogging(this IConfigurationBuilder, Action<LoggingConfiguration>)`, `ConfigureLogging(this IConfigurationBuilder, ILoggerFactory)`, `ConfigureLogging(this IConfigurationBuilder, LogLevel)`, `UseLoggerFactory(this ILoggingConfigurationBuilder, ILoggerFactory)`, `AddLoggerProvider(this ILoggingConfigurationBuilder, ILoggerProvider)`, `WithMinimumLevel(this ILoggingConfigurationBuilder, LogLevel)`; all return `ILoggingConfigurationBuilder`.
  - `internal static partial class IcyLog` (namespace `Icy.Diagnostics.Logging`), extended by Tasks 4 and 5.
  - `public sealed class DebuggerLoggerProvider : ILoggerProvider` (namespace `Icy.Diagnostics.Logging`).
  - Tests: `CapturingLoggerFactory` with `IReadOnlyList<CapturedLog> Entries`; `record CapturedLog(string Category, LogLevel Level, EventId EventId, string Message, Exception? Exception)`.

- [ ] **Step 1: Add the package**

`sources/Directory.Packages.props`, in the `Core` item group:
```xml
    <PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.12" />
```
`sources/IcyUI/IcyUI.csproj`, next to the CommunityToolkit references:
```xml
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
```

- [ ] **Step 2: Write the test helper**

`sources/IcyUI.Tests/Logging/CapturingLoggerFactory.cs`:
```csharp
using Microsoft.Extensions.Logging;

namespace Icy.Tests.Logging
{
    /// <summary>One message written through a <see cref="CapturingLoggerFactory"/>.</summary>
    public sealed record CapturedLog(string Category, LogLevel Level, EventId EventId, string Message, Exception? Exception);

    /// <summary>An <see cref="ILoggerFactory"/> that records every enabled message, for asserting what IcyUI logs.</summary>
    public sealed class CapturingLoggerFactory(LogLevel minimumLevel = LogLevel.Trace) : ILoggerFactory
    {
        private readonly List<CapturedLog> entries = [];

        public IReadOnlyList<CapturedLog> Entries => entries;

        public void AddProvider(ILoggerProvider provider) => throw new NotSupportedException();

        public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);

        public void Dispose()
        {
        }

        private sealed class Logger(CapturingLoggerFactory owner, string category) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= owner.minimumLevel && logLevel != LogLevel.None;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel))
                    owner.entries.Add(new CapturedLog(category, logLevel, eventId, formatter(state, exception), exception));
            }
        }
    }
}
```

- [ ] **Step 3: Write the failing tests**

`sources/IcyUI.Tests/Logging/LoggingConfigurationTests.cs`:
```csharp
using System.Diagnostics;
using Icy.Configuration;
using Icy.Diagnostics.Logging;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Icy.Tests.Logging
{
    public class LoggingConfigurationTests
    {
        private static IConfigurationBuilder NewBuilder() =>
            new IcyConfigurationBuilder().ConfigureRendering(new FakeRenderContext()).ConfigureInput(new FakeInputSystem());

        [Fact]
        public void Logging_DefaultsToNullFactory()
        {
            IcyConfiguration configuration = NewBuilder().Build();

            Assert.Same(NullLoggerFactory.Instance, configuration.Logging);
        }

        [Fact]
        public void UseLoggerFactory_IsUsedAsIs()
        {
            var factory = new CapturingLoggerFactory();

            IcyConfiguration configuration = NewBuilder().ConfigureLogging(factory).Build();

            Assert.Same(factory, configuration.Logging);
        }

        [Fact]
        public void Providers_ReceiveMessagesAtOrAboveTheMinimumLevel()
        {
            var sink = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder()
                .ConfigureLogging()
                .AddLoggerProvider(new FactoryProvider(sink))
                .Build();
            ILogger logger = configuration.Logging.CreateLogger("Icy.Test");

            logger.LogInformation("hidden");
            logger.LogWarning("shown");

            CapturedLog entry = Assert.Single(sink.Entries);
            Assert.Equal("shown", entry.Message);
            Assert.Equal("Icy.Test", entry.Category);
        }

        [Fact]
        public void ConfigureLogging_Level_LowersTheMinimum()
        {
            var sink = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder()
                .ConfigureLogging(LogLevel.Debug)
                .AddLoggerProvider(new FactoryProvider(sink))
                .Build();

            configuration.Logging.CreateLogger("Icy.Test").LogDebug("detail");

            Assert.Single(sink.Entries);
        }

        [Fact]
        public void ConfigureLogging_Action_ReachesTheSection()
        {
            IConfigurationBuilder builder = NewBuilder();

            builder.ConfigureLogging(l => l.MinimumLevel = LogLevel.Error);

            Assert.Equal(LogLevel.Error, builder.ConfigureLogging().Logging.MinimumLevel);
        }

        [Fact]
        public void DebuggerLoggerProvider_IsEnabledOnlyWhileADebuggerLogs()
        {
            using var provider = new DebuggerLoggerProvider();

            Assert.Equal(Debugger.IsLogging(), provider.CreateLogger("Icy.Test").IsEnabled(LogLevel.Warning));
            Assert.False(provider.CreateLogger("Icy.Test").IsEnabled(LogLevel.None));
        }

        private sealed class FactoryProvider(ILoggerFactory sink) : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName) => sink.CreateLogger(categoryName);

            public void Dispose()
            {
            }
        }
    }
}
```
- [ ] **Step 4: Run them to see them fail**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~LoggingConfigurationTests"`
Expected: build FAILS: `IcyConfiguration` has no `Logging`, `ConfigureLogging` doesn't exist, `Icy.Diagnostics.Logging`
doesn't exist.

- [ ] **Step 5: Implement the section**

`sources/IcyUI/Configuration/LoggingConfiguration.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.Logging;

namespace Icy.Configuration
{
    /// <summary>
    /// Describes where IcyUI's log messages go: either a whole <see cref="ILoggerFactory"/> supplied by the game, or
    /// IcyUI's own minimal factory feeding the <see cref="Providers"/> at or above <see cref="MinimumLevel"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The engine integrations add a default provider (Stride's log on Stride, the debugger's output window on
    /// MonoGame). Set <see cref="Factory"/> to send everything to your own logging setup instead, for example a
    /// factory created with <c>LoggerFactory.Create(b =&gt; b.AddConsole())</c>.
    /// </para>
    /// <para>
    /// IcyUI never disposes a <see cref="Factory"/> it didn't create.
    /// </para>
    /// </remarks>
    public sealed class LoggingConfiguration
    {
        /// <summary>
        /// Gets or sets a logger factory that replaces IcyUI's own; when set, <see cref="MinimumLevel"/> and
        /// <see cref="Providers"/> are ignored and filtering is the factory's job.
        /// </summary>
        public ILoggerFactory? Factory { get; set; }

        /// <summary>
        /// Gets or sets the lowest level IcyUI's own factory passes to <see cref="Providers"/>. Defaults to
        /// <see cref="LogLevel.Warning"/>.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Warning;

        /// <summary>
        /// Gets the providers IcyUI's own factory writes to. Empty means nothing is logged.
        /// </summary>
        public IList<ILoggerProvider> Providers { get; } = [];
    }
}
```

`sources/IcyUI/Configuration/ILoggingConfigurationBuilder.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace Icy.Configuration
{
    /// <summary>
    /// The logging section of an <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public interface ILoggingConfigurationBuilder : IConfigurationBuilder
    {
        /// <summary>
        /// Gets the logging options the built configuration's <see cref="IcyConfiguration.Logging"/> is created from.
        /// </summary>
        LoggingConfiguration Logging { get; }
    }
}
```

`IConfigurationBuilder.cs`, after `ConfigureScaling()`:
```csharp
        /// <summary>
        /// Switches to the logging section, creating default <see cref="LoggingConfiguration"/> options if needed.
        /// </summary>
        /// <returns>The logging section of this builder.</returns>
        ILoggingConfigurationBuilder ConfigureLogging();
```

`IcyConfigurationBuilder.cs`: add `ILoggingConfigurationBuilder` to the implemented interfaces, then:
```csharp
        /// <inheritdoc/>
        [NotNull]
        public LoggingConfiguration? Logging { get; private set; }

        /// <inheritdoc/>
        public ILoggingConfigurationBuilder ConfigureLogging()
        {
            Logging ??= new LoggingConfiguration();
            return this;
        }
```
and in `Build()`, after `ConfigureScaling();`:
```csharp
            ConfigureLogging();
            ILoggerFactory loggerFactory = Logging.Factory
                ?? (Logging.Providers.Count == 0 ? NullLoggerFactory.Instance : new IcyLoggerFactory(Logging.MinimumLevel, Logging.Providers));
            return new IcyConfiguration(InputSystem, Assets, RenderContext, Types, Theme, Scaling, loggerFactory);
```
(add `using Icy.Diagnostics.Logging; using Microsoft.Extensions.Logging; using Microsoft.Extensions.Logging.Abstractions;`).

`IcyConfiguration.cs`: add the parameter, its `<param>` doc, the property and the assignment **before**
`Fonts = new(this);`:
```csharp
        /// <param name="logging">
        /// Where IcyUI writes its log messages. Defaults to <see cref="NullLoggerFactory.Instance"/> (nothing is logged)
        /// when omitted.
        /// </param>
        public IcyConfiguration(IInputSystem input, AssetConfiguration assets, IRenderContext renderContext, ReflectionConfiguration types, ThemeConfiguration? theme = null, ScalingConfiguration? scaling = null, ILoggerFactory? logging = null)
        {
            ...
            Logging = logging ?? NullLoggerFactory.Instance;
            Fonts = new(this);
        }

        /// <summary>
        /// Gets the factory IcyUI creates its loggers from. Categories are IcyUI's full type names, such as
        /// <c>Icy.Rendering.Fonts.FontSystem</c>, so a prefix filter like <c>"Icy.Markup"</c> selects one area.
        /// </summary>
        public ILoggerFactory Logging { get; }
```

`BuildingExtensions.cs`, a new region of extensions:
```csharp
        /// <summary>
        /// Configures the logging options with a callback.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="configure">Receives the <see cref="LoggingConfiguration"/>.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder ConfigureLogging(this IConfigurationBuilder builder, Action<LoggingConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            ILoggingConfigurationBuilder section = builder.ConfigureLogging();
            configure(section.Logging);
            return section;
        }

        /// <summary>
        /// Sends IcyUI's log messages to <paramref name="factory"/> instead of IcyUI's own providers.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="factory">The factory to create IcyUI's loggers from. IcyUI doesn't dispose it.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder ConfigureLogging(this IConfigurationBuilder builder, ILoggerFactory factory) =>
            builder.ConfigureLogging().UseLoggerFactory(factory);

        /// <summary>
        /// Sets the lowest level IcyUI's own providers receive; <see cref="LogLevel.Debug"/> shows why something doesn't render.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="minimumLevel">The lowest level to log.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder ConfigureLogging(this IConfigurationBuilder builder, LogLevel minimumLevel) =>
            builder.ConfigureLogging().WithMinimumLevel(minimumLevel);

        /// <inheritdoc cref="ConfigureLogging(IConfigurationBuilder, ILoggerFactory)"/>
        public static ILoggingConfigurationBuilder UseLoggerFactory(this ILoggingConfigurationBuilder builder, ILoggerFactory factory)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(factory);
            builder.Logging.Factory = factory;
            return builder;
        }

        /// <summary>
        /// Adds a provider to IcyUI's own logger factory.
        /// </summary>
        /// <param name="builder">The logging section.</param>
        /// <param name="provider">The provider to write to.</param>
        /// <returns>The logging section of the builder.</returns>
        public static ILoggingConfigurationBuilder AddLoggerProvider(this ILoggingConfigurationBuilder builder, ILoggerProvider provider)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(provider);
            builder.Logging.Providers.Add(provider);
            return builder;
        }

        /// <inheritdoc cref="ConfigureLogging(IConfigurationBuilder, LogLevel)"/>
        public static ILoggingConfigurationBuilder WithMinimumLevel(this ILoggingConfigurationBuilder builder, LogLevel minimumLevel)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Logging.MinimumLevel = minimumLevel;
            return builder;
        }
```

`sources/IcyUI/Diagnostics/Logging/IcyLoggerFactory.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.Logging;

namespace Icy.Diagnostics.Logging
{
    /// <summary>
    /// IcyUI's own minimal <see cref="ILoggerFactory"/>: writes to a fixed set of providers at or above a minimum level.
    /// Exists so core needs only the logging abstractions, not <c>Microsoft.Extensions.Logging</c> itself.
    /// </summary>
    internal sealed class IcyLoggerFactory(LogLevel minimumLevel, IEnumerable<ILoggerProvider> providers) : ILoggerFactory
    {
        private readonly ILoggerProvider[] providers = [.. providers];

        public void AddProvider(ILoggerProvider provider) =>
            throw new NotSupportedException("Add providers through the configuration builder's logging section.");

        public ILogger CreateLogger(string categoryName) =>
            new Logger(minimumLevel, [.. providers.Select(p => p.CreateLogger(categoryName))]);

        public void Dispose()
        {
        }

        private sealed class Logger(LogLevel minimumLevel, ILogger[] loggers) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel)
            {
                if (logLevel == LogLevel.None || logLevel < minimumLevel)
                    return false;
                foreach (ILogger logger in loggers)
                {
                    if (logger.IsEnabled(logLevel))
                        return true;
                }

                return false;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.None || logLevel < minimumLevel)
                    return;
                foreach (ILogger logger in loggers)
                {
                    if (logger.IsEnabled(logLevel))
                        logger.Log(logLevel, eventId, state, exception, formatter);
                }
            }
        }
    }
}
```

`sources/IcyUI/Diagnostics/Logging/DebuggerLoggerProvider.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Icy.Diagnostics.Logging
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> that writes to an attached debugger's output window, such as Visual Studio's
    /// Output pane. IcyUI.MonoGame uses it by default.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Messages are written with <see cref="Debugger.Log(int, string, string)"/> only while
    /// <see cref="Debugger.IsLogging"/> is <see langword="true"/>; without a debugger every logger reports itself
    /// disabled, so logging costs one check. Unlike <see cref="Debug.WriteLine(string)"/>, which is compiled out of
    /// Release builds of the calling assembly, this works from IcyUI's Release package.
    /// </para>
    /// </remarks>
    public sealed class DebuggerLoggerProvider : ILoggerProvider
    {
        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => new DebuggerLogger(categoryName);

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        private sealed class DebuggerLogger(string category) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && Debugger.IsLogging();

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;
                string message = $"{logLevel}: {category}: {formatter(state, exception)}";
                if (exception != null)
                    message += Environment.NewLine + exception;
                Debugger.Log((int)logLevel, category, message + Environment.NewLine);
            }
        }
    }
}
```

`sources/IcyUI/Diagnostics/Logging/IcyLog.cs` (Tasks 4 and 5 add the messages):
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.Logging;

namespace Icy.Diagnostics.Logging
{
    /// <summary>
    /// Every message IcyUI logs, as source-generated methods: a call costs one <see cref="ILogger.IsEnabled"/> check
    /// and no formatting when nothing listens. Event ids: 1xxx fonts, 2xxx bindings, 3xxx types.
    /// </summary>
    internal static partial class IcyLog
    {
    }
}
```

- [ ] **Step 6: Run the tests**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~LoggingConfigurationTests"`
Expected: 6 passed.

- [ ] **Step 7: Full suite and zero warnings**

Run: `dotnet build sources/IcyUI/IcyUI.csproj -c Release` → `0 Warning(s)` (audit warnings excepted);
`dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj > /tmp/t2.log 2>&1; tail -5 /tmp/t2.log` → `Failed: 0`, Total 1849.

- [ ] **Step 8: Commit**

```bash
git add sources/Directory.Packages.props sources/IcyUI sources/IcyUI.Tests/Logging
git commit -m "Add logging: a configuration section on Microsoft.Extensions.Logging.Abstractions"
```

---

### Task 3: Builder gaps, theme and markup loading

**Files:**
- Modify: `sources/IcyUI/Configuration/IConfigurationBuilder.cs`, `IcyConfigurationBuilder.cs`, `BuildingExtensions.cs`, `AssetConfiguration.cs`
- Modify: `sources/IcyUI/UI/Canvas.cs`
- Modify: `sources/IcyUI/Markup/MarkupImporter.cs`, `ResourceDictionaryImporter.cs` (cref to the new `AddMarkupSupport`)
- Create: `sources/IcyUI.Tests/Configuration/ConfigurationBuilderTests.cs`, `sources/IcyUI.Tests/Markup/LoadMarkupTests.cs`

**Interfaces:**
- Consumes: Task 2's builder.
- Produces:
  - `IConfigurationBuilder AfterBuild(Action<IcyConfiguration> step)` (interface member; steps run in declaration order at the end of `Build()`).
  - Extensions on `IConfigurationBuilder`: `ConfigureAssets(Action<AssetConfiguration>) : IAssetConfigurationBuilder`, `ConfigureTypes(Action<ReflectionConfiguration>) : IReflectionConfigurationBuilder`, `ConfigureInput(Action<IInputSystem>) : IInputConfigurationBuilder`, `ConfigureRendering(Action<IRenderContext>) : IRenderingConfigurationBuilder`, `ConfigureGestures(Action<GestureSettings>) : IInputConfigurationBuilder`, `ConfigureMarkup(Action<MarkupConfiguration>) : IReflectionConfigurationBuilder`, `ConfigureDiagnostics(Action<DebugToolRegistry>) : IReflectionConfigurationBuilder`, `UseDefaultTheme() : IThemeConfigurationBuilder`, `AddMarkupSupport() : IAssetConfigurationBuilder`.
  - `ConfigureTheme(Action<ThemeConfiguration>)` and `ConfigureScaling(Action<ScalingConfiguration>)` move to `IConfigurationBuilder` receivers (same return types); `ConfigureTheme`'s callback becomes deferred.
  - `ConfigureMarkup`/`ConfigureDiagnostics` move from `IReflectionConfigurationBuilder` receivers to `IConfigurationBuilder` receivers (same return type).
  - Removed: `AddMarkupSupport(this IAssetConfigurationBuilder, IcyConfiguration)`.
  - `AssetConfiguration.Load<T>(string path) : T`, `AssetConfiguration.LoadAsync<T>(string path) : ValueTask<T>` (`where T : class`).
  - `Canvas.LoadMarkup(string path, object? dataContext = null) : UIElement`.

- [ ] **Step 1: Write the failing builder tests**

`sources/IcyUI.Tests/Configuration/ConfigurationBuilderTests.cs`:
```csharp
using Icy.Configuration;
using Icy.Diagnostics;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;

namespace Icy.Tests.Configuration
{
    public class ConfigurationBuilderTests
    {
        private static IConfigurationBuilder NewBuilder() =>
            new IcyConfigurationBuilder().ConfigureRendering(new FakeRenderContext()).ConfigureInput(new FakeInputSystem());

        [Fact]
        public void AfterBuild_StepsRunInDeclarationOrderOnTheBuiltConfiguration()
        {
            var calls = new List<string>();
            IcyConfiguration? seen = null;

            IcyConfiguration built = NewBuilder()
                .AfterBuild(c => { calls.Add("first"); seen = c; })
                .AfterBuild(_ => calls.Add("second"))
                .Build();

            Assert.Equal(["first", "second"], calls);
            Assert.Same(built, seen);
        }

        [Fact]
        public void UseDefaultTheme_OnTheBuilder_LoadsTheTheme()
        {
            IcyConfiguration configuration = NewBuilder().UseDefaultTheme().Build();

            Assert.NotNull(configuration.Theme.Theme);
        }

        [Fact]
        public void ConfigureTheme_Action_RunsAfterEarlierDeferredTheme()
        {
            var custom = new ResourceDictionary();

            IcyConfiguration configuration = NewBuilder()
                .UseDefaultTheme()
                .ConfigureTheme(t => t.Theme = custom)
                .Build();

            Assert.Same(custom, configuration.Theme.Theme);
        }

        [Fact]
        public void SectionLambdas_ReachTheirSections()
        {
            IcyConfiguration configuration = NewBuilder()
                .ConfigureGestures(g => g.Slop = 42f)
                .ConfigureDiagnostics(d => Assert.NotNull(d))
                .ConfigureMarkup(m => Assert.NotNull(m))
                .ConfigureAssets(a => Assert.NotNull(a))
                .ConfigureTypes(t => Assert.NotNull(t))
                .ConfigureRendering(r => Assert.NotNull(r))
                .ConfigureInput(i => Assert.NotNull(i))
                .Build();

            Assert.Equal(42f, configuration.Input.Events.Gestures.Settings.Slop);
        }

        [Fact]
        public void AddMarkupSupport_OnTheBuilder_LetsAssetsLoadMarkup()
        {
            IcyConfiguration configuration = NewBuilder().AddMarkupSupport().Build();

            UIElement page = configuration.Assets.Load<UIElement>("Resources/TestPage.xml");

            Assert.NotNull(page);
        }
    }
}
```

`sources/IcyUI.Tests/Markup/LoadMarkupTests.cs`:
```csharp
using Icy.Configuration;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;

namespace Icy.Tests.Markup
{
    public class LoadMarkupTests
    {
        private static Canvas NewCanvas() =>
            new(new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .AddMarkupSupport()
                .Build());

        [Fact]
        public void LoadMarkup_AddsTheElementAndSetsItsDataContext()
        {
            Canvas canvas = NewCanvas();
            var data = new object();

            UIElement element = canvas.LoadMarkup("Resources/TestPage.xml", data);

            Assert.Same(canvas, element.Canvas);
            Assert.Same(data, element.DataContext);
        }

        [Fact]
        public void LoadMarkup_Twice_AddsTwoDistinctElements()
        {
            Canvas canvas = NewCanvas();

            UIElement first = canvas.LoadMarkup("Resources/TestPage.xml");
            UIElement second = canvas.LoadMarkup("Resources/TestPage.xml");

            Assert.NotSame(first, second);
        }
    }
}
```
`Canvas` has no public list of its root elements; `Canvas.Add` sets `element.Canvas`, which the test checks.
`sources/IcyUI.Tests/Resources/TestPage.xml` is already copied to the output (`NavigationServiceTests` loads it by
this path).

- [ ] **Step 2: Run them to see them fail**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~ConfigurationBuilderTests|FullyQualifiedName~LoadMarkupTests"`
Expected: build FAILS on `AfterBuild`, `UseDefaultTheme()` on the builder, `ConfigureGestures`, `AddMarkupSupport()`,
`Assets.Load`, `LoadMarkup`.

- [ ] **Step 3: Implement deferred steps**

`IConfigurationBuilder.cs`:
```csharp
        /// <summary>
        /// Records a step that needs the built configuration, such as loading the default theme. Steps run at the
        /// end of <see cref="Build"/>, in the order they were added.
        /// </summary>
        /// <param name="step">The step to run on the built configuration.</param>
        /// <returns>This builder.</returns>
        IConfigurationBuilder AfterBuild(Action<IcyConfiguration> step);
```
`IcyConfigurationBuilder.cs`:
```csharp
        private readonly List<Action<IcyConfiguration>> afterBuildSteps = [];

        /// <inheritdoc/>
        public IConfigurationBuilder AfterBuild(Action<IcyConfiguration> step)
        {
            ArgumentNullException.ThrowIfNull(step);
            afterBuildSteps.Add(step);
            return this;
        }
```
and `Build()` ends with:
```csharp
            var configuration = new IcyConfiguration(InputSystem, Assets, RenderContext, Types, Theme, Scaling, loggerFactory);
            foreach (Action<IcyConfiguration> step in afterBuildSteps)
                step(configuration);
            return configuration;
```

- [ ] **Step 4: Implement the extensions**

In `BuildingExtensions.cs`:
- Replace `ConfigureMarkup` and `ConfigureDiagnostics` receivers with `this IConfigurationBuilder builder` and their
  bodies with `IReflectionConfigurationBuilder section = builder.ConfigureTypes(); configure(section.Types.Markup); return section;`
  (`.Diagnostics` for the other). Keep their docs, adding "Doesn't require <see cref="IConfigurationBuilder.ConfigureTypes()"/> first."
- `ConfigureScaling(this IScalingConfigurationBuilder, Action<ScalingConfiguration>)`: change the receiver to
  `this IConfigurationBuilder builder` (body: `IScalingConfigurationBuilder section = builder.ConfigureScaling();
  configure(section.Scaling); return section;`). Inside `UseIcyUI(ui => ...)` the lambda's parameter is an
  `IConfigurationBuilder`, so an extension on a section interface wouldn't be callable there.
- Change `ConfigureTheme(this IThemeConfigurationBuilder builder, Action<ThemeConfiguration> configure)` to take
  `this IConfigurationBuilder builder` (same reason) and to defer:
```csharp
        /// <remarks>
        /// The callback runs when the configuration is built, in order with other deferred steps such as
        /// <see cref="UseDefaultTheme(IConfigurationBuilder)"/>, so a theme set here replaces a default theme an
        /// engine integration requested earlier.
        /// </remarks>
        public static IThemeConfigurationBuilder ConfigureTheme(this IConfigurationBuilder builder, Action<ThemeConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            builder.AfterBuild(c => configure(c.Theme));
            return builder.ConfigureTheme();
        }
```
- Remove `AddMarkupSupport(this IAssetConfigurationBuilder builder, IcyConfiguration configuration)` and add:
```csharp
        /// <summary>
        /// Lets <see cref="AssetConfiguration.Load{T}(string)"/> load markup files as <see cref="UI.UIElement"/> and
        /// <see cref="UI.ResourceDictionary"/> assets. Both engine integrations call it by default.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <returns>The asset section of the builder.</returns>
        public static IAssetConfigurationBuilder AddMarkupSupport(this IConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AfterBuild(c =>
            {
                c.Assets.AssetResolver.RegisterImporter(new MarkupImporter(c));
                c.Assets.AssetResolver.RegisterImporter(new ResourceDictionaryImporter(c));
            });
            return builder.ConfigureAssets();
        }

        /// <summary>
        /// Loads IcyUI's built-in theme once the configuration is built. Both engine integrations call it by default.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <returns>The theme section of the builder.</returns>
        public static IThemeConfigurationBuilder UseDefaultTheme(this IConfigurationBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AfterBuild(c => c.UseDefaultTheme());
            return builder.ConfigureTheme();
        }

        /// <summary>
        /// Configures gesture recognition (hold delay, multi-tap interval, movement tolerance).
        /// </summary>
        /// <param name="builder">The configuration builder; its input system must already be set.</param>
        /// <param name="configure">Receives the input system's <see cref="GestureSettings"/>.</param>
        /// <returns>The input section of the builder.</returns>
        public static IInputConfigurationBuilder ConfigureGestures(this IConfigurationBuilder builder, Action<GestureSettings> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IInputConfigurationBuilder section = builder.ConfigureInput();
            configure(section.InputSystem.Events.Gestures.Settings);
            return section;
        }
```
- Add the four immediate section lambdas, each with full XML docs, following this shape:
```csharp
        /// <summary>
        /// Configures the asset options with a callback.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="configure">Receives the <see cref="AssetConfiguration"/>.</param>
        /// <returns>The asset section of the builder.</returns>
        public static IAssetConfigurationBuilder ConfigureAssets(this IConfigurationBuilder builder, Action<AssetConfiguration> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            IAssetConfigurationBuilder section = builder.ConfigureAssets();
            configure(section.Assets);
            return section;
        }
```
  `ConfigureTypes(Action<ReflectionConfiguration>)` → `section.Types`; `ConfigureInput(Action<IInputSystem>)` →
  `section.InputSystem`; `ConfigureRendering(Action<IRenderContext>)` → `section.RenderContext`.
- Update the `<see cref>`s in `MarkupImporter.cs` and `ResourceDictionaryImporter.cs` from
  `BuildingExtensions.AddMarkupSupport(IAssetConfigurationBuilder, IcyConfiguration)` to
  `BuildingExtensions.AddMarkupSupport(IConfigurationBuilder)`, and the cref in `ReflectionConfiguration.cs` to
  `ConfigureDiagnostics(IConfigurationBuilder, Action{DebugToolRegistry})`.

- [ ] **Step 5: Implement `Assets.Load` and `LoadMarkup`**

`AssetConfiguration.cs`:
```csharp
        /// <summary>
        /// Loads an asset from a path relative to <see cref="DefaultAssetContext"/> (the application folder unless
        /// changed), through the importers registered on <see cref="AssetResolver"/>.
        /// </summary>
        /// <typeparam name="T">The asset type, such as <see cref="UI.UIElement"/> for a markup file.</typeparam>
        /// <param name="path">The asset's path, for example <c>"UI/Menu.xml"</c>.</param>
        /// <returns>The loaded asset.</returns>
        /// <exception cref="FileNotFoundException">The file doesn't exist.</exception>
        /// <exception cref="InvalidOperationException">No importer handles the file's format.</exception>
        public T Load<T>(string path)
            where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            return AssetResolver.LoadAsset<T>(DefaultAssetContext, path);
        }

        /// <summary>
        /// Loads an asset asynchronously from a path relative to <see cref="DefaultAssetContext"/>.
        /// </summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="path">The asset's path.</param>
        /// <returns>The loaded asset.</returns>
        /// <exception cref="FileNotFoundException">The file doesn't exist.</exception>
        /// <exception cref="InvalidOperationException">No importer handles the file's format.</exception>
        public ValueTask<T> LoadAsync<T>(string path)
            where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            return AssetResolver.LoadAssetAsync<T>(DefaultAssetContext, path);
        }
```
`Canvas.cs`, after `Add`:
```csharp
        /// <summary>
        /// Loads a markup file, sets its data context, and adds it to the canvas.
        /// </summary>
        /// <param name="path">The markup file's path, relative to the configuration's default asset context (the
        /// application folder unless changed). Copy the file to the output folder.</param>
        /// <param name="dataContext">The object the markup's <c>{Binding}</c> expressions read from, or
        /// <see langword="null"/> to leave <see cref="UIElement.DataContext"/> unset.</param>
        /// <returns>The loaded root element, already added to this canvas.</returns>
        /// <exception cref="FileNotFoundException">The file doesn't exist.</exception>
        /// <exception cref="InvalidOperationException">Markup support isn't enabled; the engine integrations enable
        /// it by default, otherwise call <see cref="Configuration.BuildingExtensions.AddMarkupSupport(Configuration.IConfigurationBuilder)"/>.</exception>
        /// <exception cref="Markup.MarkupException">The markup is invalid.</exception>
        public UIElement LoadMarkup(string path, object? dataContext = null)
        {
            UIElement element = Configuration.Assets.Load<UIElement>(path);
            if (dataContext != null)
                element.DataContext = dataContext;
            Add(element);
            return element;
        }
```

- [ ] **Step 6: Run the tests**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~ConfigurationBuilderTests|FullyQualifiedName~LoadMarkupTests"`
Expected: 7 passed.

- [ ] **Step 7: Fix call sites, full suite, zero warnings**

Run: `grep -rn "AddMarkupSupport(" sources --include=*.cs | grep -v /obj/` and update any caller of the removed overload
to `.AddMarkupSupport()`. Then `dotnet build sources/IcyUI.sln -c Release` (shipped libraries `0 Warning(s)` apart from
audit warnings) and `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj > /tmp/t3.log 2>&1; tail -5 /tmp/t3.log`
→ `Failed: 0`, Total 1856.

- [ ] **Step 8: Commit**

```bash
git add sources/IcyUI sources/IcyUI.Tests
git commit -m "Fill the builder's gaps: deferred steps, a lambda for every section, theme and markup support, LoadMarkup"
```

---

### Task 4: Fonts

**Files:**
- Create: `sources/IcyUI/Configuration/IFontConfigurationBuilder.cs`, `sources/IcyUI/Configuration/FontBuildingExtensions.cs`
- Modify: `IConfigurationBuilder.cs`, `IcyConfigurationBuilder.cs` (implement `IFontConfigurationBuilder`)
- Modify: `sources/IcyUI/Rendering/Fonts/FontSystem.cs`, `sources/IcyUI/UI/Controls/TextBlock.cs` (`ResolveFont`), `sources/IcyUI/UI/Controls/TextBox.cs` (its font resolution near line 330), `sources/IcyUI/Diagnostics/Logging/IcyLog.cs`
- Modify: `docfx/en/markup/index.md`, `docfx/ru/markup/index.md` are **not** touched here (Task 9)
- Create: `sources/IcyUI.Tests/Rendering/FontSystemTests.cs`, `sources/IcyUI.Tests/WindowsFactAttribute.cs`

**Interfaces:**
- Consumes: `AfterBuild` (Task 3), `IcyConfiguration.Logging`, `IcyLog` (Task 2).
- Produces:
  - `IConfigurationBuilder.ConfigureFonts() : IFontConfigurationBuilder`.
  - Extensions on `IFontConfigurationBuilder` (all deferred, all return `IFontConfigurationBuilder`): `UseSystemFonts()`, `WithDefaultFontFamily(string family)`, `ImportFont(string path)`, `WithFallbackFont(Func<IcyConfiguration, IFont> createFont)`.
  - `ConfigureFonts(this IConfigurationBuilder, Action<FontSystem>) : IFontConfigurationBuilder` (deferred).
  - `FontSystem.ResolveTextFont(string family, float size, FontStyle style) : IFont?` (internal).
  - `FontSystem.SystemFontIndexBuilds : int` (internal, for tests).
  - `IcyLog` event ids 1001 `FontNotFound`, 1002 `SystemFontImportFailed`, 1003 `SystemFontFileSkipped`, 1004 `NoDefaultFont`.

- [ ] **Step 1: Write the failing tests**

`sources/IcyUI.Tests/WindowsFactAttribute.cs`:
```csharp
namespace Icy.Tests
{
    /// <summary>A fact that needs Windows (for example the Windows fonts folder); skipped on other systems.</summary>
    public sealed class WindowsFactAttribute : FactAttribute
    {
        public WindowsFactAttribute()
        {
            if (!OperatingSystem.IsWindows())
                Skip = "Needs Windows.";
        }
    }
}
```

`sources/IcyUI.Tests/Rendering/FontSystemTests.cs`:
```csharp
using Icy.Configuration;
using Icy.Rendering.Fonts;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Microsoft.Extensions.Logging;

namespace Icy.Tests.Rendering
{
    public class FontSystemTests
    {
        private static IConfigurationBuilder NewBuilder(CapturingLoggerFactory? log = null)
        {
            IConfigurationBuilder builder = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureAssets()
                .AddBasicFontSupport();
            if (log != null)
                builder.ConfigureLogging(log);
            return builder;
        }

        private static int Count(CapturingLoggerFactory log, int eventId) => log.Entries.Count(e => e.EventId.Id == eventId);

        [Fact]
        public void ConfigureFonts_StepsApplyInBuild()
        {
            IcyConfiguration configuration = NewBuilder().ConfigureFonts().WithDefaultFontFamily("Airfool").Build();

            Assert.Equal("Airfool", configuration.Fonts.DefaultFontFamily);
        }

        [Fact]
        public void WithDefaultFontFamily_LastStepWins()
        {
            IcyConfiguration configuration = NewBuilder()
                .ConfigureFonts().WithDefaultFontFamily("Segoe UI")
                .ConfigureFonts().WithDefaultFontFamily("Airfool")
                .Build();

            Assert.Equal("Airfool", configuration.Fonts.DefaultFontFamily);
        }

        [Fact]
        public void ImportFont_Step_MakesTheFamilyResolvable()
        {
            IcyConfiguration configuration = NewBuilder()
                .ConfigureFonts().ImportFont("Resources/Airfool.otf").WithDefaultFontFamily("Airfool")
                .Build();

            Assert.NotNull(configuration.Fonts.ResolveTextFont(string.Empty, 16, FontStyle.Regular));
        }

        [Fact]
        public void GetOrLoad_UnknownFamily_WarnsOnceAndReturnsFallback()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder(log).Build();

            Assert.Null(configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular)));
            Assert.Null(configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular)));
            Assert.Null(configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 20, FontStyle.Regular)));

            CapturedLog entry = Assert.Single(log.Entries, e => e.EventId.Id == 1001);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Contains("No Such Font", entry.Message);
            Assert.Equal("Icy.Rendering.Fonts.FontSystem", entry.Category);
        }

        [Fact]
        public void GetOrLoad_UnknownFamily_IsNotRetried()
        {
            IcyConfiguration configuration = NewBuilder().ConfigureFonts().UseSystemFonts().Build();

            configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular));
            int builds = configuration.Fonts.SystemFontIndexBuilds;
            configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular));

            Assert.Equal(1, builds);
            Assert.Equal(1, configuration.Fonts.SystemFontIndexBuilds);
        }

        [Fact]
        public void ImportFont_AfterMiss_ResolvesTheFamily()
        {
            IcyConfiguration configuration = NewBuilder().Build();
            Assert.Null(configuration.Fonts.GetOrLoad(new FontInfo("Airfool", 16, FontStyle.Regular)));

            configuration.Fonts.ImportFont(configuration.Assets.DefaultAssetContext, "Resources/Airfool.otf");

            Assert.NotNull(configuration.Fonts.GetOrLoad(new FontInfo("Airfool", 16, FontStyle.Regular)));
        }

        [Fact]
        public void EnableSystemFonts_DoesNotIndexUntilFirstMiss()
        {
            IcyConfiguration configuration = NewBuilder().ConfigureFonts().UseSystemFonts().Build();

            Assert.True(configuration.Fonts.AreSystemFontsEnabled);
            Assert.Equal(0, configuration.Fonts.SystemFontIndexBuilds);

            configuration.Fonts.GetOrLoad(new FontInfo("No Such Font", 16, FontStyle.Regular));
            configuration.Fonts.GetOrLoad(new FontInfo("Another Missing Font", 16, FontStyle.Regular));

            Assert.Equal(1, configuration.Fonts.SystemFontIndexBuilds);
        }

        [Fact]
        public void ResolveTextFont_NoDefaultFamilyAndNoFallback_WarnsOnce()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder(log).Build();

            Assert.Null(configuration.Fonts.ResolveTextFont(string.Empty, 16, FontStyle.Regular));
            Assert.Null(configuration.Fonts.ResolveTextFont(string.Empty, 16, FontStyle.Regular));

            Assert.Equal(1, Count(log, 1004));
        }

        [WindowsFact]
        public void EngineDefaultFontSteps_ResolveSegoeUi()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder(log)
                .ConfigureFonts().UseSystemFonts().WithDefaultFontFamily("Segoe UI")
                .Build();

            IFont? font = configuration.Fonts.ResolveTextFont(string.Empty, 16, FontStyle.Bold);

            Assert.NotNull(font);
            Assert.Equal("Segoe UI", font.Info.Family);
            Assert.DoesNotContain(log.Entries, e => e.Level >= LogLevel.Warning);
        }
    }
}
```

- [ ] **Step 2: Run them to see them fail**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~FontSystemTests"`
Expected: build FAILS on `ConfigureFonts`, `ResolveTextFont`, `SystemFontIndexBuilds`.

- [ ] **Step 3: Fonts section**

`sources/IcyUI/Configuration/IFontConfigurationBuilder.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace Icy.Configuration
{
    /// <summary>
    /// The fonts section of an <see cref="IConfigurationBuilder"/>. The font system exists only once the configuration
    /// is built, so every step in this section is deferred with <see cref="IConfigurationBuilder.AfterBuild"/> and
    /// runs in declaration order.
    /// </summary>
    public interface IFontConfigurationBuilder : IConfigurationBuilder
    {
    }
}
```
`IConfigurationBuilder.cs`:
```csharp
        /// <summary>
        /// Switches to the fonts section.
        /// </summary>
        /// <returns>The fonts section of this builder.</returns>
        IFontConfigurationBuilder ConfigureFonts();
```
`IcyConfigurationBuilder.cs`: implement `IFontConfigurationBuilder`; `public IFontConfigurationBuilder ConfigureFonts() => this;`
with `/// <inheritdoc/>`.

`sources/IcyUI/Configuration/FontBuildingExtensions.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Rendering.Fonts;

namespace Icy.Configuration
{
    /// <summary>
    /// Font steps for <see cref="IFontConfigurationBuilder"/>. Each runs on the built configuration's
    /// <see cref="IcyConfiguration.Fonts"/>, in declaration order.
    /// </summary>
    public static class FontBuildingExtensions
    {
        /// <summary>
        /// Configures the font system with a callback once the configuration is built.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="configure">Receives the built <see cref="FontSystem"/>.</param>
        /// <returns>The fonts section of the builder.</returns>
        public static IFontConfigurationBuilder ConfigureFonts(this IConfigurationBuilder builder, Action<FontSystem> configure)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(configure);
            builder.AfterBuild(c => configure(c.Fonts));
            return builder.ConfigureFonts();
        }

        /// <summary>
        /// Lets fonts installed on this system be found by family name (<see cref="FontSystem.EnableSystemFonts"/>).
        /// </summary>
        /// <param name="builder">The fonts section.</param>
        /// <returns>The fonts section.</returns>
        /// <remarks>
        /// On Windows this is the Windows fonts folder. On Linux and macOS, .NET maps it to the user's font folder
        /// (<c>~/.fonts</c>, <c>~/Library/Fonts</c>), so ship the font with the game and use
        /// <see cref="ImportFont(IFontConfigurationBuilder, string)"/> there instead.
        /// </remarks>
        public static IFontConfigurationBuilder UseSystemFonts(this IFontConfigurationBuilder builder) =>
            builder.Defer(f => f.EnableSystemFonts());

        /// <summary>
        /// Sets the family text uses when it doesn't name one (<see cref="FontSystem.DefaultFontFamily"/>).
        /// </summary>
        /// <param name="builder">The fonts section.</param>
        /// <param name="family">The family name stored in the font file, such as <c>"Segoe UI"</c>.</param>
        /// <returns>The fonts section.</returns>
        public static IFontConfigurationBuilder WithDefaultFontFamily(this IFontConfigurationBuilder builder, string family)
        {
            ArgumentNullException.ThrowIfNull(family);
            return builder.Defer(f => f.DefaultFontFamily = family);
        }

        /// <summary>
        /// Imports a font file shipped with the game, relative to the default asset context (the application folder
        /// unless changed).
        /// </summary>
        /// <param name="builder">The fonts section.</param>
        /// <param name="path">The font file's path, for example <c>"Fonts/MyFont.ttf"</c>. Copy it to the output folder.</param>
        /// <returns>The fonts section.</returns>
        /// <remarks>The file is loaded when the configuration is built; a missing file throws there.</remarks>
        public static IFontConfigurationBuilder ImportFont(this IFontConfigurationBuilder builder, string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            ArgumentNullException.ThrowIfNull(builder);
            builder.AfterBuild(c => c.Fonts.ImportFont(c.Assets.DefaultAssetContext, path));
            return builder;
        }

        /// <summary>
        /// Sets the font used when nothing else resolves (<see cref="FontSystem.FallbackFont"/>).
        /// </summary>
        /// <param name="builder">The fonts section.</param>
        /// <param name="createFont">Creates the font from the built configuration.</param>
        /// <returns>The fonts section.</returns>
        public static IFontConfigurationBuilder WithFallbackFont(this IFontConfigurationBuilder builder, Func<IcyConfiguration, IFont> createFont)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(createFont);
            builder.AfterBuild(c => c.Fonts.FallbackFont = createFont(c));
            return builder;
        }

        private static IFontConfigurationBuilder Defer(this IFontConfigurationBuilder builder, Action<FontSystem> step)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.AfterBuild(c => step(c.Fonts));
            return builder;
        }
    }
}
```

- [ ] **Step 4: Log messages**

Add to `IcyLog`:
```csharp
        [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "No font is loaded for family '{Family}' ({Style}): text using it falls back to FontSystem.FallbackFont, or doesn't render when there's none. Import the font file with FontSystem.ImportFont, or call EnableSystemFonts() if it's installed on this system.")]
        public static partial void FontNotFound(ILogger logger, string family, Icy.Rendering.Fonts.FontStyle style);

        [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "The system font '{Family}' ({Style}) failed to import; text using it falls back to FontSystem.FallbackFont.")]
        public static partial void SystemFontImportFailed(ILogger logger, string family, Icy.Rendering.Fonts.FontStyle style, Exception exception);

        [LoggerMessage(EventId = 1003, Level = LogLevel.Debug, Message = "Skipped '{FileName}' in the system fonts folder: it isn't a readable font.")]
        public static partial void SystemFontFileSkipped(ILogger logger, string fileName, Exception exception);

        [LoggerMessage(EventId = 1004, Level = LogLevel.Warning, Message = "Text has no font: FontSystem.DefaultFontFamily is empty and FallbackFont is null, so text without its own FontFamily doesn't render. Set a default family (the engine integrations use \"Segoe UI\").")]
        public static partial void NoDefaultFont(ILogger logger);
```

- [ ] **Step 5: FontSystem changes**

In `FontSystem.cs` (add `using Icy.Diagnostics.Logging; using Microsoft.Extensions.Logging;`):

Fields, replacing `systemFontFileNames`'s declaration area:
```csharp
        // (family, style) pairs that resolved to nothing; text resolves its font every frame, so a miss must not retry.
        private readonly HashSet<VectorFontInfo> missingFonts = [];

        private FrozenDictionary<VectorFontInfo, string>? systemFontFileNames;
        private bool systemFontsEnabled;
        private bool noDefaultFontReported;
        private ILogger? logger;
```
Properties and methods:
```csharp
        /// <summary>
        /// Gets a value indicating whether fonts installed on the system can be found by family name.
        /// </summary>
        /// <remarks>The system fonts folder is indexed on the first lookup that needs it, not when this turns on.</remarks>
        public bool AreSystemFontsEnabled => systemFontsEnabled;

        /// <summary>Gets how many times the system fonts folder was indexed (tests).</summary>
        internal int SystemFontIndexBuilds { get; private set; }

        private ILogger Logger => logger ??= config.Logging.CreateLogger<FontSystem>();

        private FrozenDictionary<VectorFontInfo, string> SystemFontIndex => systemFontFileNames ??= BuildSystemFontIndex();
```
`EnableSystemFonts()` becomes (keep and extend its docs: "Cheap: the folder is indexed on the first font lookup
that misses, once (typically 200–400 ms on Windows)."):
```csharp
        public void EnableSystemFonts() => systemFontsEnabled = true;
```
`GetOrLoad` body:
```csharp
            if (fontsCache.TryGetValue(info, out var font))
                return font;

            if (TryGetRasterizationData(info, out var pair))
                return ReuseFont(info, pair);

            if (missingFonts.Contains(info))
                return FallbackFont;

            missingFonts.Add(info);
            if (AreSystemFontsEnabled)
            {
                try
                {
                    if (TryImportSystemFont(info, out font))
                    {
                        missingFonts.Remove(info);
                        return font;
                    }
                }
                catch (Exception ex)
                {
                    IcyLog.SystemFontImportFailed(Logger, info.Family, info.Style, ex);
                    return FallbackFont;
                }
            }

            IcyLog.FontNotFound(Logger, info.Family, info.Style);
            return FallbackFont;
```
`ImportSystemFont` keeps its public contract, built on a new helper:
```csharp
        public IFont ImportSystemFont(FontInfo info)
        {
            if (!AreSystemFontsEnabled)
                return ThrowHelper.ThrowInvalidOperationException<IFont>($"Couldn't import a system fonts when system fonts support is disabled. Try calling {nameof(EnableSystemFonts)} first.");

            return TryImportSystemFont(info, out IFont? font)
                ? font
                : ThrowHelper.ThrowArgumentException<IFont>("Couldn't find system font with the specified font info.");
        }

        private bool TryImportSystemFont(FontInfo info, [NotNullWhen(true)] out IFont? font)
        {
            font = null;
            if (!SystemFontIndex.TryGetValue(info, out string? systemPath))
                systemPath = info.Family;

            if (!SystemFontsAssetContext.IsAvailable(systemPath))
                return false;

            ImportFont(SystemFontsAssetContext, systemPath);
            if (!TryGetRasterizationData(info, out var data))
                return ThrowHelper.ThrowInvalidOperationException<bool>("Font was imported but its rasterization data couldn't be found.");
            font = ReuseFont(info, data);
            return true;
        }

        private FrozenDictionary<VectorFontInfo, string> BuildSystemFontIndex()
        {
            SystemFontIndexBuilds++;
            return EnumerateSystemFonts().DistinctBy(x => x.Info).ToFrozenDictionary(x => x.Info, y => y.FileName);
        }
```
`EnumerateSystemFonts` becomes an instance method; it returns nothing when the folder doesn't exist, and logs the
skipped files:
```csharp
        private IEnumerable<(VectorFontInfo Info, string FileName)> EnumerateSystemFonts()
        {
            if (string.IsNullOrEmpty(SystemFontsPath) || !Directory.Exists(SystemFontsPath))
                yield break;

            foreach (string filePath in Directory.EnumerateFiles(SystemFontsPath))
            {
                string fileName = Path.GetFileName(filePath);
                if (!DynamicFontsHelper.IsFontFileName(fileName))
                    continue;

                FontInfo[] infos;
                try
                {
                    infos = DynamicFontsHelper.GetFontInfo(filePath);
                }
                catch (Exception ex)
                {
                    // (keep the existing comment about misleading extensions)
                    IcyLog.SystemFontFileSkipped(Logger, fileName, ex);
                    continue;
                }

                if (infos.Length == 0)
                    continue;

                yield return ((VectorFontInfo)infos[0], fileName);
            }
        }
```
`Clear()`: add `missingFonts.Clear();`. (`RegisterFont` needs no change: `GetOrLoad` checks the cache and the
imported fonts' rasterization data before the miss set, so a font imported after a miss is found. Keep that order;
`ImportFont_AfterMiss_ResolvesTheFamily` pins it.)
New internal method:
```csharp
        /// <summary>
        /// Resolves the font a text element draws with: its own family, else <see cref="DefaultFontFamily"/>, else
        /// <see cref="FallbackFont"/>.
        /// </summary>
        /// <param name="family">The element's own family; empty when it has none.</param>
        /// <param name="size">The font size.</param>
        /// <param name="style">The font style.</param>
        /// <returns>The font, or <see langword="null"/> when nothing resolves.</returns>
        internal IFont? ResolveTextFont(string family, float size, FontStyle style)
        {
            if (family.Length == 0)
                family = DefaultFontFamily;
            if (family.Length > 0)
                return GetOrLoad(new FontInfo(family, size, style));

            if (FallbackFont == null && !noDefaultFontReported)
            {
                noDefaultFontReported = true;
                IcyLog.NoDefaultFont(Logger);
            }

            return FallbackFont;
        }
```
Remove the `[MemberNotNullWhen(true, nameof(systemFontFileNames))]` attribute from `AreSystemFontsEnabled`.

`TextBlock.ResolveFont()` body:
```csharp
            return Configuration?.Fonts?.ResolveTextFont(FontFamily, FontSize, FontStyle);
```
`TextBox`: replace its `family.Length == 0 ? fonts.FallbackFont : fonts.GetOrLoad(new FontInfo(family, FontSize, FontStyle.Regular))`
logic with `return fonts.ResolveTextFont(FontFamily, FontSize, FontStyle.Regular);` (keep the null check on
`Configuration?.Fonts` that precedes it; read lines 315–332 first).

- [ ] **Step 6: Run the tests**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~FontSystemTests"`
Expected: 9 passed on Windows.

- [ ] **Step 7: Full suite and zero warnings**

`dotnet build sources/IcyUI/IcyUI.csproj -c Release` → `0 Warning(s)`;
`dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj > /tmp/t4.log 2>&1; tail -5 /tmp/t4.log` → `Failed: 0`, Total 1865.

- [ ] **Step 8: Commit**

```bash
git add sources/IcyUI sources/IcyUI.Tests
git commit -m "Fonts: a builder section, lazy system-font indexing, cached misses and missing-font warnings"
```

---

### Task 5: Binding and assembly-scan warnings

**Files:**
- Modify: `sources/IcyUI/Data/Bindings/DynamicPropertyPath.cs`, `sources/IcyUI/Markup/Extensions/BindingExtension.cs`, `sources/IcyUI/Data/AssemblyResolver.cs`, `sources/IcyUI/Configuration/IcyConfiguration.cs`, `IcyLog.cs`
- Create: `sources/IcyUI.Tests/Data/BindingLoggingTests.cs`, `sources/IcyUI.Tests/Data/AssemblyResolverTests.cs`

**Interfaces:**
- Consumes: `IcyLog`, `CapturingLoggerFactory`, `IcyConfiguration.Logging`.
- Produces:
  - `DynamicPropertyPath(string path, ILogger? logger)` constructor (the existing one-argument constructor stays and calls it with `null`).
  - `AssemblyResolver.Logger : ILogger` (public, settable, default `NullLogger.Instance`).
  - internal test seams on `AssemblyResolver`: `Func<IEnumerable<Assembly>> AssembliesProvider`, `Func<Assembly, Type[]> TypesProvider`.
  - `IcyLog` ids 2001 `BindingMemberNotFound`, 3001 `AssemblyScanIncomplete`.

- [ ] **Step 1: Write the failing tests**

`sources/IcyUI.Tests/Data/BindingLoggingTests.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Configuration;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Icy.Tests.Rendering;
using Icy.UI.Controls;

namespace Icy.Tests.Data
{
    public class BindingLoggingTests
    {
        public sealed class TitleSource : ObservableObject
        {
            private string title = "Play";

            public string Title
            {
                get => title;
                set => SetProperty(ref title, value);
            }
        }

        private static TextBlock Load(string path, CapturingLoggerFactory log)
        {
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureLogging(log)
                .Build();
            return (TextBlock)new MarkupLoader(configuration).Load(
                $"<TextBlock xmlns=\"https://icyui.dev/markup\" Text=\"{{Binding {path}}}\"/>", "x.xml");
        }

        [Fact]
        public void MissingMember_WarnsOncePerBinding()
        {
            var log = new CapturingLoggerFactory();
            TextBlock text = Load("Titel", log);
            var source = new TitleSource();

            text.DataContext = source;
            source.Title = "Pause";
            text.DataContext = new TitleSource();

            CapturedLog entry = Assert.Single(log.Entries, e => e.EventId.Id == 2001);
            Assert.Contains("Titel", entry.Message);
            Assert.Contains(nameof(TitleSource), entry.Message);
        }

        [Fact]
        public void ExistingMember_DoesNotWarn()
        {
            var log = new CapturingLoggerFactory();
            TextBlock text = Load("Title", log);

            text.DataContext = new TitleSource();

            Assert.Equal("Play", text.Text);
            Assert.DoesNotContain(log.Entries, e => e.EventId.Id == 2001);
        }

        [Fact]
        public void NoDataContext_DoesNotWarn()
        {
            var log = new CapturingLoggerFactory();
            Load("Titel", log);

            Assert.DoesNotContain(log.Entries, e => e.EventId.Id == 2001);
        }
    }
}
```

`sources/IcyUI.Tests/Data/AssemblyResolverTests.cs`:
```csharp
using System.Reflection;
using Icy.Data;
using Icy.Tests.Logging;
using Icy.UI.Controls;

namespace Icy.Tests.Data
{
    public class AssemblyResolverTests
    {
        private static AssemblyResolver ResolverWithABrokenAssembly(CapturingLoggerFactory log)
        {
            Assembly broken = typeof(AssemblyResolverTests).Assembly;
            Assembly icy = typeof(Button).Assembly;
            return new AssemblyResolver
            {
                DefaultAssembly = broken,
                Logger = log.CreateLogger("Icy.Data.AssemblyResolver"),
                AssembliesProvider = () => [broken, icy],
                TypesProvider = a => a == broken
                    ? throw new ReflectionTypeLoadException([null], [new TypeLoadException("A dependency is missing.")])
                    : a.GetTypes(),
            };
        }

        [Fact]
        public void FindType_KeepsSearchingPastAnAssemblyThatFailsToLoadTypes()
        {
            AssemblyResolver resolver = ResolverWithABrokenAssembly(new CapturingLoggerFactory());

            Assert.Equal(typeof(Button), resolver.FindType("Button"));
        }

        [Fact]
        public void FindType_WarnsOncePerBrokenAssembly()
        {
            var log = new CapturingLoggerFactory();
            AssemblyResolver resolver = ResolverWithABrokenAssembly(log);

            resolver.FindType("Button");
            resolver.FindType("TextBlock");

            CapturedLog entry = Assert.Single(log.Entries, e => e.EventId.Id == 3001);
            Assert.Contains("IcyUI.Tests", entry.Message);
        }
    }
}
```

- [ ] **Step 2: Run them to see them fail**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~BindingLoggingTests|FullyQualifiedName~AssemblyResolverTests"`
Expected: build FAILS on `AssemblyResolver.Logger`, `AssembliesProvider`, `TypesProvider`.

- [ ] **Step 3: Log messages**

Add to `IcyLog`:
```csharp
        [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Binding path '{Path}': '{Member}' isn't a public property of '{SourceType}', so the binding reads null. Check the spelling and that the property is public.")]
        public static partial void BindingMemberNotFound(ILogger logger, string path, string member, string sourceType);

        [LoggerMessage(EventId = 3001, Level = LogLevel.Warning, Message = "Some types in assembly '{Assembly}' couldn't be loaded (usually a missing dependency), so markup can't find them.")]
        public static partial void AssemblyScanIncomplete(ILogger logger, string assembly, Exception exception);
```

- [ ] **Step 4: DynamicPropertyPath**

Add the field, constructor and helper; make `CallGet`/`CallSet` instance methods and report a missing member in
their non-indexer branch:
```csharp
        private readonly ILogger? logger;
        private bool missingMemberReported;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPropertyPath"/> class that reports a member missing on
        /// the source, once, to <paramref name="logger"/>.
        /// </summary>
        /// <param name="path">The dot-separated property path.</param>
        /// <param name="logger">Where to warn about a member the source doesn't have, or <see langword="null"/>.</param>
        public DynamicPropertyPath(string path, ILogger? logger)
            : this(path)
        {
            this.logger = logger;
        }

        private void ReportMissingMember(object current, string member)
        {
            if (logger == null || missingMemberReported)
                return;
            missingMemberReported = true;
            IcyLog.BindingMemberNotFound(logger, displayPath, member, current.GetType().Name);
        }
```
In `CallGet`'s final branch:
```csharp
            else if (segment != "this")
            {
                var property = current?.GetType().GetProperty(segment, BindingFlags.Instance | BindingFlags.Public);
                if (property == null && current != null)
                    ReportMissingMember(current, segment);
                current = property?.GetValue(current);
            }
```
and the same `if (property == null && current != null) ReportMissingMember(current, segment);` in `CallSet`'s final
branch, before `property?.SetValue(...)`. Remove `static` from `CallGet`, `CallSet`.

`BindingExtension.ProvideValue`: create the path with the configuration's logger:
```csharp
            var binding = new Binding(new DynamicPropertyPath(
                string.IsNullOrEmpty(Path) ? "this" : Path,
                context.Configuration.Logging.CreateLogger<Binding>()));
```
(`using Icy.Data.Bindings; using Microsoft.Extensions.Logging;` as needed.)

- [ ] **Step 5: AssemblyResolver**

Replace `FindType` and add the members (keep and complete the XML docs; document `Logger`):
```csharp
        private readonly HashSet<Assembly> reportedAssemblies = [];

        /// <summary>
        /// Gets or sets where the resolver warns about assemblies whose types can't all be loaded.
        /// <see cref="IcyConfiguration"/> sets it from <see cref="IcyConfiguration.Logging"/>.
        /// </summary>
        public ILogger Logger { get; set; } = NullLogger.Instance;

        internal Func<IEnumerable<Assembly>> AssembliesProvider { get; set; } = AppDomain.CurrentDomain.GetAssemblies;

        internal Func<Assembly, Type[]> TypesProvider { get; set; } = a => a.GetTypes();

        public Type? FindType(string name)
        {
            if (typesCache.TryGetValue(name, out Type? value))
                return value;

            Type? result = TryGetType(name) ?? DefaultAssembly.GetType(name);
            if (result == null)
            {
                foreach (Assembly assembly in AssembliesProvider())
                {
                    result = assembly.GetType(name) ?? FindByShortName(assembly, name);
                    if (result != null)
                        break;
                }
            }

            return typesCache[name] = result;
        }

        private static Type? TryGetType(string name)
        {
            try
            {
                return Type.GetType(name);
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or BadImageFormatException)
            {
                return null;
            }
        }

        // Only types with a parameterless constructor: markup creates the element itself.
        private Type? FindByShortName(Assembly assembly, string name)
        {
            Type?[] types;
            try
            {
                types = TypesProvider(assembly);
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
                if (reportedAssemblies.Add(assembly))
                    IcyLog.AssemblyScanIncomplete(Logger, assembly.GetName().Name ?? assembly.FullName ?? "?", ex);
            }

            return types.OfType<Type>().Where(x => x.GetConstructor(Type.EmptyTypes) != null).FirstOrDefault(x => x.Name == name || x.FullName == name);
        }
```
`IcyConfiguration` constructor, after `Logging = …`:
```csharp
            if (types.AssemblyResolver is AssemblyResolver resolver)
                resolver.Logger = Logging.CreateLogger<AssemblyResolver>();
```

- [ ] **Step 6: Run the tests**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~BindingLoggingTests|FullyQualifiedName~AssemblyResolverTests"`
Expected: 5 passed.

- [ ] **Step 7: Full suite and zero warnings**

`dotnet build sources/IcyUI/IcyUI.csproj -c Release` → `0 Warning(s)`;
`dotnet test … > /tmp/t5.log 2>&1; tail -5 /tmp/t5.log` → `Failed: 0`, Total 1870.

- [ ] **Step 8: Commit**

```bash
git add sources/IcyUI sources/IcyUI.Tests
git commit -m "Warn about binding paths that miss on their source, and keep scanning past a broken assembly"
```

---

### Task 6: Text as content

**Files:**
- Modify: `sources/IcyUI/Data/TypeConversionManager.cs` (`TryGetCastMethod`), `sources/IcyUI/UI/UIElement.cs`
- Create: `sources/IcyUI.Tests/Data/TextContentConversionTests.cs`

**Interfaces:**
- Produces: `public static implicit operator UIElement?(string? text)` on `UIElement`; `TypeConversionManager` finds
  `op_Implicit`/`op_Explicit` declared on the target type as well as the source type.

- [ ] **Step 1: Write the failing tests**

`sources/IcyUI.Tests/Data/TextContentConversionTests.cs`:
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Data
{
    public class TextContentConversionTests
    {
        public sealed class TitleSource : ObservableObject
        {
            private string title = "Play";

            public string Title
            {
                get => title;
                set => SetProperty(ref title, value);
            }
        }

        private static Button LoadButton(string attributes) =>
            (Button)new MarkupLoader(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()))
                .Load($"<Button xmlns=\"https://icyui.dev/markup\" {attributes}/>", "x.xml");

        [Fact]
        public void StringContent_InCode_BecomesATextBlock()
        {
            var button = new Button { Content = "Play" };

            Assert.Equal("Play", Assert.IsType<TextBlock>(button.Content).Text);
        }

        [Fact]
        public void NullString_ConvertsToNull()
        {
            var button = new Button { Content = "Play" };
            string? none = null;

            button.Content = none;

            Assert.Null(button.Content);
        }

        [Fact]
        public void StringAttribute_InMarkup_BecomesATextBlock()
        {
            Button button = LoadButton("Content=\"Play\"");

            Assert.Equal("Play", Assert.IsType<TextBlock>(button.Content).Text);
        }

        [Fact]
        public void StringBinding_FollowsTheSource()
        {
            Button button = LoadButton("Content=\"{Binding Title}\"");
            var source = new TitleSource();

            button.DataContext = source;
            Assert.Equal("Play", Assert.IsType<TextBlock>(button.Content).Text);

            source.Title = "Pause";
            Assert.Equal("Pause", Assert.IsType<TextBlock>(button.Content).Text);
        }
    }
}
```

- [ ] **Step 2: Run them to see them fail**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj --filter "FullyQualifiedName~TextContentConversionTests"`
Expected: build FAILS (`Content = "Play"`: cannot convert `string` to `UIElement`).

- [ ] **Step 3: Add the operator**

`UIElement.cs`, in the class body near its other public statics (or at the end):
```csharp
        /// <summary>
        /// Converts text to a <see cref="TextBlock"/> showing it, so a string can be used wherever an element is
        /// expected: <c>new Button { Content = "Play" }</c>, <c>panel.Children.Add("Score: 0")</c>.
        /// </summary>
        /// <param name="text">The text to show; <see langword="null"/> converts to <see langword="null"/>.</param>
        /// <returns>A new <see cref="TextBlock"/> whose <see cref="TextBlock.Text"/> is <paramref name="text"/>.</returns>
        /// <remarks>
        /// <para>
        /// Markup attributes (<c>Content="Play"</c>) and bindings (<c>Content="{Binding Title}"</c>) convert the same
        /// way. A binding creates a new <see cref="TextBlock"/> each time its value changes; for text that changes
        /// often, put a <see cref="TextBlock"/> in the content and bind its <see cref="TextBlock.Text"/> instead.
        /// </para>
        /// </remarks>
        public static implicit operator UIElement?(string? text) => text is null ? null : new TextBlock { Text = text };
```
(`using Icy.UI.Controls;` if `UIElement.cs` lacks it; `TextBlock` is in `Icy.UI.Controls`.)

- [ ] **Step 4: Run the tests**

Same command. Expected: `StringContent_InCode…` and `NullString…` pass; the markup and binding tests still FAIL with
"Can't find any converters between System.String and Icy.UI.UIElement" / `BindingException`.

- [ ] **Step 5: Look up operators on the target type**

`TypeConversionManager.TryGetCastMethod`:
```csharp
        private static bool TryGetCastMethod(Type sourceType, Type targetType, [NotNullWhen(true)] out Func<object, object>? castMethod)
        {
            // Operators can be declared on either side: string has none to UIElement, UIElement declares one from string.
            MethodInfo? method = FindCastMethod(sourceType, sourceType, targetType) ?? FindCastMethod(targetType, sourceType, targetType);
            if (method != null)
            {
                var paramExpression = Expression.Parameter(typeof(object), "value");
                var castExpression = Expression.Convert(paramExpression, method.GetParameters()[0].ParameterType);
                var operatorExpression = Expression.Convert(Expression.Call(method, castExpression), typeof(object));
                castMethod = Expression.Lambda<Func<object, object>>(operatorExpression, paramExpression).Compile();
                return true;
            }

            castMethod = null;
            return false;
        }

        private static MethodInfo? FindCastMethod(Type declaringType, Type sourceType, Type targetType) =>
            declaringType.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(x => IsCastMethod(x, sourceType, targetType));
```
(`Expression.Convert` on the result boxes value-type results, which `TypeAs`/no conversion didn't; reference results
are unaffected. `IsCastMethod` stays as is.)

- [ ] **Step 6: Run the tests**

Same command. Expected: 4 passed.

- [ ] **Step 7: Full suite and zero warnings**

`dotnet build sources/IcyUI.sln -c Release` (shipped libraries `0 Warning(s)`; an ambiguity error anywhere means a call
site passed a string to an overload set that also takes `UIElement`: report it, don't paper over it);
`dotnet test … > /tmp/t6.log 2>&1; tail -5 /tmp/t6.log` → `Failed: 0`, Total 1874.

- [ ] **Step 8: Commit**

```bash
git add sources/IcyUI sources/IcyUI.Tests
git commit -m "Let text stand in for an element: string converts to a TextBlock in code, markup and bindings"
```

---

### Task 7: The host contract and the MonoGame host

**Files:**
- Create: `sources/IcyUI/UI/IIcyHost.cs`
- Create: `sources/IcyUI.MonoGame/Configuration/MonoGameIcyHost.cs`; delete `sources/IcyUI.MonoGame/Configuration/MonoGameIcyRenderer.cs`
- Modify: `sources/IcyUI.MonoGame/Configuration/MonoGameBuildingExtensions.cs`
- Modify: `sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs` (only enough to compile; Task 9 rewrites it)

**Interfaces:**
- Consumes: Tasks 2–4 (`AddMarkupSupport()`, `UseDefaultTheme()`, `ConfigureFonts()…`, `AddLoggerProvider`, `DebuggerLoggerProvider`).
- Produces:
  - `public interface IIcyHost { IcyConfiguration Configuration { get; } Canvas MainCanvas { get; } IList<Canvas> Canvases { get; } }` (namespace `Icy.UI`).
  - `public sealed class MonoGameIcyHost : DrawableGameComponent, IIcyHost` (namespace `Icy.MonoGame.Configuration`).
  - On `Game`: `UseIcyUI() : MonoGameIcyHost`, `UseIcyUI(Action<IConfigurationBuilder>) : MonoGameIcyHost`, `GetIcyHost() : MonoGameIcyHost`, `TryGetIcyHost(out MonoGameIcyHost?) : bool`, `GetIcyConfiguration() : IcyConfiguration`, `GetIcyCanvas() : Canvas`.

There are no unit tests for the engine side (the test project can't create a MonoGame `Game`); the verification is
the Release build, the docs snippet project compiling, and Ivan's smoke test.

- [ ] **Step 1: The contract**

`sources/IcyUI/UI/IIcyHost.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;

namespace Icy.UI
{
    /// <summary>
    /// IcyUI running inside a game: its configuration and the canvases the engine draws every frame. Returned by each
    /// engine integration's <c>UseIcyUI()</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Engine-independent game code can take an <see cref="IIcyHost"/>; the engine's own host type adds
    /// engine-specific settings.
    /// </para>
    /// <para>
    /// A canvas you render yourself with <see cref="Canvas.Render"/> (for example into a render target) must not be in
    /// <see cref="Canvases"/>, or it's drawn twice.
    /// </para>
    /// </remarks>
    public interface IIcyHost
    {
        /// <summary>Gets the configuration every element in this game is created with.</summary>
        IcyConfiguration Configuration { get; }

        /// <summary>Gets the canvas <c>UseIcyUI()</c> created, with input enabled; <see cref="Canvases"/>[0].</summary>
        Canvas MainCanvas { get; }

        /// <summary>Gets the canvases drawn every frame, in order: a later canvas draws on top of an earlier one.</summary>
        IList<Canvas> Canvases { get; }
    }
}
```

- [ ] **Step 2: The MonoGame host**

Delete `MonoGameIcyRenderer.cs`; create `MonoGameIcyHost.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;
using Icy.Input;
using Icy.MonoGame.Rendering;
using Icy.UI;
using Microsoft.Xna.Framework;

namespace Icy.MonoGame.Configuration
{
    /// <summary>
    /// IcyUI in a MonoGame game: a game component that pumps input, follows the display scale and draws
    /// <see cref="Canvases"/> every frame. Created by <see cref="MonoGameBuildingExtensions.UseIcyUI(Game)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Components draw during <c>base.Draw()</c> in your game's <c>Draw</c>, and <see cref="DrawableGameComponent.DrawOrder"/>
    /// defaults to <see cref="int.MaxValue"/>, so the UI covers what your game and its other components draw. Anything
    /// you draw after <c>base.Draw()</c> lands on top of the UI.
    /// </para>
    /// </remarks>
    public sealed class MonoGameIcyHost : DrawableGameComponent, IIcyHost
    {
        internal MonoGameIcyHost(Game game, IcyConfiguration configuration)
            : base(game)
        {
            Configuration = configuration;
            MainCanvas = new Canvas(configuration) { IsInputEnabled = true };
            Canvases = [MainCanvas];
            DrawOrder = int.MaxValue;
        }

        /// <inheritdoc/>
        public IcyConfiguration Configuration { get; }

        /// <inheritdoc/>
        public Canvas MainCanvas { get; }

        /// <inheritdoc/>
        public IList<Canvas> Canvases { get; }

        /// <inheritdoc/>
        public override void Initialize()
        {
            Configuration.Input.Initialize();
            base.Initialize();
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            if (Configuration.Input is IUpdateableInput updateable)
                updateable.Update(gameTime.ElapsedGameTime);

            (Configuration.RenderContext as RenderContext)?.PollDisplayScale();
            base.Update(gameTime);
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime)
        {
            // By index: a canvas added or removed by UI code during this frame mustn't break the loop.
            for (int i = 0; i < Canvases.Count; i++)
                Canvases[i].Render();
            base.Draw(gameTime);
        }
    }
}
```

- [ ] **Step 3: The extensions**

In `MonoGameBuildingExtensions.cs` replace `UseIcyUI(Game)`, `UseIcyUI(Game, Action<IConfigurationBuilder>)` and
`GetIcyConfiguration`:
```csharp
        /// <summary>
        /// Sets IcyUI up in the game with the default MonoGame configuration: the default theme, markup support,
        /// system fonts with <c>"Segoe UI"</c>, and log messages in the debugger's output window.
        /// </summary>
        /// <param name="game">The game; call this in <c>Initialize()</c>, before <c>base.Initialize()</c>.</param>
        /// <returns>The host, whose <see cref="IIcyHost.MainCanvas"/> is ready for elements.</returns>
        /// <exception cref="InvalidOperationException">IcyUI is already set up for this game.</exception>
        public static MonoGameIcyHost UseIcyUI(this Game game) => game.UseIcyUI(static _ => { });

        /// <summary>
        /// Sets IcyUI up in the game: the default MonoGame configuration first, then <paramref name="configure"/>.
        /// </summary>
        /// <param name="game">The game; call this in <c>Initialize()</c>, before <c>base.Initialize()</c>.</param>
        /// <param name="configure">Changes the defaults, for example <c>ui =&gt; ui.ConfigureFonts().WithDefaultFontFamily("Arial")</c>.</param>
        /// <returns>The host, whose <see cref="IIcyHost.MainCanvas"/> is ready for elements.</returns>
        /// <exception cref="InvalidOperationException">IcyUI is already set up for this game.</exception>
        public static MonoGameIcyHost UseIcyUI(this Game game, Action<IConfigurationBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(configure);
            if (game.TryGetIcyHost(out _))
                return ThrowHelper.ThrowInvalidOperationException<MonoGameIcyHost>("IcyUI is already set up for this game; use GetIcyHost() to reach it.");

            var builder = new IcyConfigurationBuilder();
            builder.WithDefaultMonoGameConfiguration(game);
            configure(builder);
            var host = new MonoGameIcyHost(game, builder.Build());
            game.Components.Add(host);
            return host;
        }

        /// <summary>
        /// Gets the IcyUI host of the game, if <see cref="UseIcyUI(Game)"/> ran.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <param name="host">The host, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> when IcyUI is set up.</returns>
        public static bool TryGetIcyHost(this Game game, [NotNullWhen(true)] out MonoGameIcyHost? host)
        {
            ArgumentNullException.ThrowIfNull(game);
            host = game.Components.OfType<MonoGameIcyHost>().FirstOrDefault();
            return host != null;
        }

        /// <summary>
        /// Gets the IcyUI host of the game.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <returns>The host <see cref="UseIcyUI(Game)"/> created.</returns>
        /// <exception cref="InvalidOperationException">IcyUI isn't set up yet: call <see cref="UseIcyUI(Game)"/> first.</exception>
        public static MonoGameIcyHost GetIcyHost(this Game game) =>
            game.TryGetIcyHost(out MonoGameIcyHost? host)
                ? host
                : ThrowHelper.ThrowInvalidOperationException<MonoGameIcyHost>("IcyUI isn't set up for this game yet. Call UseIcyUI() first.");

        /// <summary>Gets the configuration IcyUI runs with in the game.</summary>
        /// <param name="game">The game.</param>
        /// <returns>The host's <see cref="IIcyHost.Configuration"/>.</returns>
        /// <exception cref="InvalidOperationException">IcyUI isn't set up yet: call <see cref="UseIcyUI(Game)"/> first.</exception>
        public static IcyConfiguration GetIcyConfiguration(this Game game) => game.GetIcyHost().Configuration;

        /// <summary>Gets the main canvas of the game.</summary>
        /// <param name="game">The game.</param>
        /// <returns>The host's <see cref="IIcyHost.MainCanvas"/>.</returns>
        /// <exception cref="InvalidOperationException">IcyUI isn't set up yet: call <see cref="UseIcyUI(Game)"/> first.</exception>
        public static Canvas GetIcyCanvas(this Game game) => game.GetIcyHost().MainCanvas;
```
`WithDefaultMonoGameConfiguration` keeps its chain and then adds the defaults (document them in its `<remarks>` as a
`<list type="bullet">`):
```csharp
            builder.ConfigureRendering(renderContext)
                   .ConfigureInput(new InputSystem(game))
                   .ConfigureTypes()
                   .ConfigureAssets()
                   .WithAssetContextFactory(new MonoGameAssetContextFactory(game))
                   .AddBasicFontSupport()
                   .UseMonoGameImporters(game);
            builder.AddMarkupSupport();
            builder.UseDefaultTheme();
            builder.ConfigureFonts().UseSystemFonts().WithDefaultFontFamily("Segoe UI");
            builder.ConfigureLogging().AddLoggerProvider(new DebuggerLoggerProvider());
            return builder;
```
Usings: `System.Diagnostics.CodeAnalysis`, `Icy.Diagnostics.Logging`, `Icy.UI`.

- [ ] **Step 4: Keep the MonoGame snippet compiling**

In `sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs`, region `game`, replace the `Initialize` body with:
```csharp
            MonoGameIcyHost ui = this.UseIcyUI();
            canvas = ui.MainCanvas;

            base.Initialize();
```
and delete the `canvas.Render();` line in `Draw` (Task 9 rewrites the file).

- [ ] **Step 5: Build**

Run:
```
dotnet build sources/IcyUI.MonoGame/IcyUI.MonoGame.csproj -c Release
dotnet build sources/IcyUI.Docs.Snippets.MonoGame/IcyUI.Docs.Snippets.MonoGame.csproj -c Release
```
Expected: `0 Warning(s)` (audit warnings excepted), `0 Error(s)`.

- [ ] **Step 6: Move the MonoGame sample to the host**

In `sources/MonoGame Sample/SampleGame.cs`, `Initialize()`, replace
```csharp
            this.UseIcyUI();

            uiConfiguration = this.GetIcyConfiguration();
            uiConfiguration.UseDefaultTheme();
            canvas = new(uiConfiguration) { IsInputEnabled = true };
```
with
```csharp
            MonoGameIcyHost ui = this.UseIcyUI();
            uiConfiguration = ui.Configuration;
            canvas = ui.MainCanvas;
```
and in `Draw` delete `canvas.Render();` (the host draws it during `base.Draw`). Keep the font import in
`LoadContent` as it is.

Run:
```
dotnet build "sources/MonoGame Sample/MonoGame Sample.csproj"
dotnet build "sources/MonoGame Sample/MonoGame Sample.csproj" -p:MonoGameBackend=DesktopGL
dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj > /tmp/t7.log 2>&1; tail -5 /tmp/t7.log
```
Expected: both builds succeed; `Failed: 0`, Total 1874.

- [ ] **Step 7: Commit**

```bash
git add sources/IcyUI/UI/IIcyHost.cs sources/IcyUI.MonoGame sources/IcyUI.Docs.Snippets.MonoGame "sources/MonoGame Sample/SampleGame.cs"
git commit -m "Add IIcyHost; UseIcyUI on MonoGame returns a host that draws its canvases"
```

---

### Task 8: The Stride host on IGame

**Files:**
- Create: `sources/IcyUI.Stride/Configuration/StrideIcyHost.cs`; delete `sources/IcyUI.Stride/Configuration/IcyUIGameSystem.cs`
- Create: `sources/IcyUI.Stride/Diagnostics/StrideLoggerProvider.cs`
- Modify: `sources/IcyUI.Stride/Configuration/StrideBuildingExtensions.cs`, `IcyUISceneRenderer.cs` (remarks mention `IcyUIGameSystem` and `UseIcyUI(Game)`)
- Modify: `sources/IcyUI.Stride/Assets/Importers/TextureImporter.cs`, `sources/IcyUI.Stride/Assets/StrideAssetContextFactory.cs`, `sources/IcyUI.Stride/Rendering/StrideNativeWindow.cs` (`Game` → `IGame`)
- Modify: `sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs` (only enough to compile)

**Interfaces:**
- Consumes: `IIcyHost` (Task 7), builder defaults (Tasks 2–4).
- Produces:
  - `public sealed class StrideIcyHost : GameSystemBase, IIcyHost` with `SceneRenderer : IcyUISceneRenderer`.
  - On `IGame`: `UseIcyUI() : StrideIcyHost`, `UseIcyUI(Action<IConfigurationBuilder>) : StrideIcyHost`, `GetIcyHost()`, `TryGetIcyHost(out StrideIcyHost?)`, `GetIcyConfiguration()`, `GetIcyCanvas()`.
  - `WithDefaultStrideConfiguration(this IConfigurationBuilder, IGame)`, `UseStrideImporters(this IAssetConfigurationBuilder, IGame)`.

- [ ] **Step 1: `IGame` internals**

- `StrideBuildingExtensions` needs `using Stride.Engine;` for `SceneSystem` and `using Stride.Games;` for `IGame`.
- `TextureImporter(Game game)` → `TextureImporter(IGame game)`; `StrideAssetContextFactory(Game game)` →
  `(IGame game)`; `StrideNativeWindow.GetInfo(Game game)` → `GetInfo(IGame game)` (update its `<param>`).
  They use only `GraphicsDevice`, `Window`, `Content`, which `IGame` has. Add `using Stride.Games;`.

- [ ] **Step 2: Stride log provider**

`sources/IcyUI.Stride/Diagnostics/StrideLoggerProvider.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.Logging;
using StrideLogger = Stride.Core.Diagnostics.Logger;

namespace Icy.Stride.Diagnostics
{
    /// <summary>
    /// Forwards IcyUI's log messages to Stride's <c>GlobalLogger</c>, so they appear with Stride's own in the game
    /// console and Game Studio's output. IcyUI.Stride's default log provider.
    /// </summary>
    internal sealed class StrideLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) =>
            new Logger(global::Stride.Core.Diagnostics.GlobalLogger.GetLogger(categoryName));

        public void Dispose()
        {
        }

        private sealed class Logger(StrideLogger logger) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                string message = formatter(state, exception);
                switch (logLevel)
                {
                    case LogLevel.Trace: logger.Verbose(message, exception); break;
                    case LogLevel.Debug: logger.Debug(message, exception); break;
                    case LogLevel.Information: logger.Info(message, exception); break;
                    case LogLevel.Warning: logger.Warning(message, exception); break;
                    case LogLevel.Error: logger.Error(message, exception); break;
                    case LogLevel.Critical: logger.Fatal(message, exception); break;
                }
            }
        }
    }
}
```
If StyleCop rejects the one-line `case` bodies, put each statement and `break;` on their own lines. If the 4.4
`Logger` methods differ (check `~/.nuget/packages/stride.core/4.4.0/lib/net10.0/Stride.Core.xml` for
`M:Stride.Core.Diagnostics.Logger.Warning`), adapt the calls.

- [ ] **Step 3: The Stride host**

Delete `IcyUIGameSystem.cs`; create `StrideIcyHost.cs`:
```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;
using Icy.Input;
using Icy.UI;
using Stride.Core;
using Stride.Games;

namespace Icy.Stride.Configuration
{
    /// <summary>
    /// IcyUI in a Stride game: a game system that pumps input and follows the display scale, with
    /// <see cref="SceneRenderer"/> drawing <see cref="Canvases"/> on top of the scene. Created by
    /// <see cref="StrideBuildingExtensions.UseIcyUI(IGame)"/>.
    /// </summary>
    public sealed class StrideIcyHost : GameSystemBase, IIcyHost
    {
        internal StrideIcyHost(IServiceRegistry registry, IcyConfiguration configuration, IcyUISceneRenderer sceneRenderer)
            : base(registry)
        {
            Configuration = configuration;
            SceneRenderer = sceneRenderer;
            MainCanvas = new Canvas(configuration) { IsInputEnabled = true };
            sceneRenderer.Canvases.Add(MainCanvas);
        }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public IcyConfiguration Configuration { get; }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public Canvas MainCanvas { get; }

        /// <inheritdoc/>
        [DataMemberIgnore]
        public IList<Canvas> Canvases => SceneRenderer.Canvases;

        /// <summary>
        /// Gets the render stage that draws <see cref="Canvases"/>, wrapped around the graphics compositor's game
        /// renderer by <see cref="StrideBuildingExtensions.UseIcyUI(IGame)"/>.
        /// </summary>
        [DataMemberIgnore]
        public IcyUISceneRenderer SceneRenderer { get; }

        /// <inheritdoc/>
        public override void Initialize()
        {
            Configuration.Input.Initialize();
            base.Initialize();
            Enabled = true;
        }

        /// <inheritdoc/>
        public override void Update(GameTime gameTime)
        {
            if (Configuration.Input is IUpdateableInput updateable)
                updateable.Update(gameTime.Elapsed);

            (Configuration.RenderContext as Rendering.RenderContext)?.PollDisplayScale();
            base.Update(gameTime);
        }
    }
}
```

- [ ] **Step 4: The extensions**

In `StrideBuildingExtensions.cs` replace `UseIcyUI(Game)`, `UseIcyUI(Game, Action)`, `GetIcyConfiguration(Game)` with
`IGame` versions mirroring Task 7 Step 3 (same docs, wording "call this from a startup script's `Start()`, or after
`BeginRun()` in a code-only game"), with this body for the configuring overload:
```csharp
        public static StrideIcyHost UseIcyUI(this IGame game, Action<IConfigurationBuilder> configure)
        {
            ArgumentNullException.ThrowIfNull(game);
            ArgumentNullException.ThrowIfNull(configure);
            if (game.TryGetIcyHost(out _))
                return ThrowHelper.ThrowInvalidOperationException<StrideIcyHost>("IcyUI is already set up for this game; use GetIcyHost() to reach it.");

            GraphicsCompositor compositor = game.Services.GetService<SceneSystem>()?.GraphicsCompositor
                ?? ThrowHelper.ThrowInvalidOperationException<GraphicsCompositor>("UseIcyUI() needs the scene's graphics compositor: call it from a startup script's Start(), or after BeginRun() in a code-only game.");

            var builder = new IcyConfigurationBuilder();
            builder.WithDefaultStrideConfiguration(game);
            configure(builder);
            IcyConfiguration configuration = builder.Build();

            var overlay = new IcyUISceneRenderer { Configuration = configuration };
            var host = new StrideIcyHost(game.Services, configuration, overlay);
            game.GameSystems.Add(host);
            compositor.Game = new CompositeSceneRenderer(compositor.Game, overlay);
            return host;
        }
```
`TryGetIcyHost` uses `game.GameSystems.OfType<StrideIcyHost>().FirstOrDefault()`; `GetIcyHost`, `GetIcyConfiguration`,
`GetIcyCanvas` as in Task 7. `WithDefaultStrideConfiguration(this IConfigurationBuilder builder, IGame game)` and
`UseStrideImporters(…, IGame game)` take `IGame`; the former ends with the same four default lines as MonoGame but
`.AddLoggerProvider(new StrideLoggerProvider())`. Usings: `Stride.Games`, `Icy.Stride.Diagnostics`, `Icy.UI`,
`System.Diagnostics.CodeAnalysis`.

Fix `IcyUISceneRenderer`'s remarks: `UseIcyUI(global::Stride.Engine.Game)` → `UseIcyUI(global::Stride.Games.IGame)`,
`IcyUIGameSystem` → `StrideIcyHost`.

- [ ] **Step 5: Keep the Stride snippets compiling**

`sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs`: in `startup-script`, replace the `Start()` body with
`Game.UseIcyUI().MainCanvas.Add(new Button { Content = new TextBlock { Text = "Hello, IcyUI" } });`; in `code-only`,
replace the three IcyUI lines with `this.UseIcyUI();`. Task 9 rewrites both.

- [ ] **Step 6: Move the Stride sample to the host**

In `sources/Stride Sample/SampleGame.cs` replace
```csharp
            IcyUISceneRenderer overlay = this.UseIcyUI();
            var configuration = this.GetIcyConfiguration();
            configuration.UseDefaultTheme();
            canvas = new Canvas(configuration) { IsInputEnabled = true };
            overlay.Canvases.Add(canvas);
```
with
```csharp
            StrideIcyHost ui = this.UseIcyUI();
            IcyConfiguration configuration = ui.Configuration;
            canvas = ui.MainCanvas;
```
(add `using Icy.Configuration;` if missing).

- [ ] **Step 7: Build**

Run:
```
dotnet build sources/IcyUI.Stride/IcyUI.Stride.csproj -c Release
dotnet build sources/IcyUI.Docs.Snippets.Stride/IcyUI.Docs.Snippets.Stride.csproj -c Release
dotnet build "sources/Stride Sample/Stride Sample.csproj"
dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj > /tmp/t8.log 2>&1; tail -5 /tmp/t8.log
```
Expected: `0 Warning(s)` for `IcyUI.Stride` (audit excepted), all builds succeed; `Failed: 0`, Total 1874.

- [ ] **Step 8: Commit**

```bash
git add sources/IcyUI.Stride sources/IcyUI.Docs.Snippets.Stride "sources/Stride Sample/SampleGame.cs"
git commit -m "Stride: UseIcyUI on IGame returns a host; IcyUI logs into Stride's log"
```

---


### Task 9: Docs, READMEs, CLAUDE.md

**Files:**
- Modify: `sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs`, `sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs`
- Create: `sources/IcyUI.Docs.Snippets.Stride/Fonts.cs`
- Modify: `docfx/en/getting-started/{stride,monogame,first-screen}.md`, `docfx/ru/getting-started/{stride,monogame,first-screen}.md`
- Modify: `docfx/en/markup/index.md`, `docfx/ru/markup/index.md` (Text content section)
- Modify: `sources/IcyUI.MonoGame/README.md`, `sources/IcyUI.Stride/README.md`, `CLAUDE.md`

- [ ] **Step 1: Stride snippets**

Rewrite the three regions of `sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs` (keep the file's first comment line):
```csharp
namespace MyGame.StartupScriptExample
{
    #region startup-script
    using Icy.Stride.Configuration;
    using Icy.UI.Controls;
    using Stride.Engine;

    /// <summary>Add this script to any entity in your startup scene: it sets up IcyUI and shows a button.</summary>
    public class IcyUIStartup : StartupScript
    {
        public override void Start()
        {
            StrideIcyHost ui = Game.UseIcyUI();
            ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });
        }
    }
    #endregion
}

namespace MyGame.CodeOnlyExample
{
    #region code-only
    using Icy.Stride.Configuration;
    using Icy.UI.Controls;
    using Stride.Engine;

    /// <summary>A code-only game (no Game Studio project): set up IcyUI once the scene system exists.</summary>
    public class MyGame : Game
    {
        protected override void BeginRun()
        {
            base.BeginRun();

            // Set up your graphics compositor and scene here, before UseIcyUI().
            StrideIcyHost ui = this.UseIcyUI();
            ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });
        }
    }
    #endregion
}

namespace MyGame.FirstScreenExample
{
    #region first-screen
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Icy.Stride.Configuration;
    using Stride.Engine;

    /// <summary>
    /// The menu's data: what the markup's {Binding} expressions read. ObservableObject raises PropertyChanged, so the
    /// screen updates when Title changes.
    /// </summary>
    public sealed class MenuViewModel : ObservableObject
    {
        private string title = "My Game";

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public ICommand PlayCommand { get; } = new RelayCommand(() => { /* Start the game. */ });

        public ICommand QuitCommand { get; } = new RelayCommand(() => { /* Exit. */ });
    }

    /// <summary>Replaces IcyUIStartup: loads Menu.xml (copied to the output folder) onto the main canvas.</summary>
    public class MenuStartup : StartupScript
    {
        public override void Start() =>
            Game.UseIcyUI().MainCanvas.LoadMarkup("Menu.xml", new MenuViewModel());
    }
    #endregion
}
```
(Check the current file's namespace layout first and keep one namespace per example as it does; if the code-only
example currently shares a namespace with another, split as above so `MyGame` the class doesn't clash.)

`sources/IcyUI.Docs.Snippets.Stride/Fonts.cs`:
```csharp
// Examples for docfx/*/getting-started/*.md ("Fonts on other platforms"). Each #region is one example on a page.
namespace MyGame.OwnFontExample
{
    using Icy.Configuration;
    using Icy.Stride.Configuration;
    using Stride.Engine;

    public class FontStartup : StartupScript
    {
        public override void Start()
        {
            #region own-font
            Game.UseIcyUI(ui => ui
                .ConfigureFonts()
                .ImportFont("Fonts/MyFont.ttf")           // copied to the output folder
                .WithDefaultFontFamily("My Font"));        // the family name stored in the file
            #endregion
        }
    }
}
```

- [ ] **Step 2: MonoGame snippets**

Rewrite `sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs`:
```csharp
// Examples for docfx/*/getting-started/monogame.md and first-screen.md. Each #region is one example on a page.
namespace MyGame.GameExample
{
    #region game
    using Icy.MonoGame.Configuration;
    using Icy.UI.Controls;
    using Microsoft.Xna.Framework;

    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            MonoGameIcyHost ui = this.UseIcyUI();
            ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });

            base.Initialize();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime); // IcyUI draws here, on top of what you drew above.
        }
    }
    #endregion
}

namespace MyGame.FirstScreenExample
{
    #region first-screen
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Icy.MonoGame.Configuration;
    using Microsoft.Xna.Framework;

    /// <summary>The menu's data: what the markup's {Binding} expressions read.</summary>
    public sealed class MenuViewModel : ObservableObject
    {
        private string title = "My Game";

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public ICommand PlayCommand { get; } = new RelayCommand(() => { /* Start the game. */ });

        public ICommand QuitCommand { get; } = new RelayCommand(() => { /* Exit. */ });
    }

    public class Game1 : Game
    {
        public Game1()
        {
            _ = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Loads Menu.xml (copied to the output folder) onto the main canvas.
            this.UseIcyUI().MainCanvas.LoadMarkup("Menu.xml", new MenuViewModel());
            base.Initialize();
        }
    }
    #endregion
}
```
Build both snippet projects in Release: `0 Error(s)`.

- [ ] **Step 3: English pages**

`docfx/en/getting-started/stride.md` — replace the whole page with:
````markdown
# Getting started with Stride

This page puts an IcyUI button on screen in a Stride game. Most Stride games are Game Studio projects, so that path
comes first; if you write your game in code only, skip to [Without Game Studio](#without-game-studio).

## In a Game Studio project

1. **Create a project** in the Stride launcher (Stride 4.4 or later; any template works, for example "New Game").

2. **Add the package** to the game project, `MyGame/MyGame.csproj` (the shared one, not the platform project):

   ```
   dotnet add MyGame/MyGame.csproj package IcyUI.Stride --prerelease
   ```

3. **Add a startup script.** Create `IcyUIStartup.cs` in the game project:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#startup-script)]

   `UseIcyUI()` adds IcyUI to the game and draws it on top of whatever your scene renders. It returns the host:
   `ui.MainCanvas` is the canvas your elements go on, and `ui.Configuration` holds every setting. The defaults load
   the built-in theme and use the system font Segoe UI. A string used as content becomes a <xref:Icy.UI.Controls.TextBlock>.

4. **Attach the script.** Reopen the project in Game Studio and let it reload the scripts. In your startup scene,
   select an entity (the camera is fine), then in the property grid choose **Add component** and pick `IcyUIStartup`.

5. **Run the game.** A "Hello, IcyUI" button appears over the scene and reacts to the mouse and keyboard.

Other scripts reach the same UI with `Game.GetIcyCanvas()` or `Game.GetIcyHost()`.

## Without Game Studio

If you derive your own `Game` class and set up the scene in code, set up IcyUI in `BeginRun()`, after the graphics
compositor exists:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#code-only)]

`UseIcyUI()` draws through `SceneSystem.GraphicsCompositor`, so it must not be `null` at that point.

## Fonts on other platforms

IcyUI doesn't ship a font: by default it uses Segoe UI from the Windows fonts folder. On Linux and macOS, or to use
your own font everywhere, ship a font file with the game (set its **Copy to Output Directory** to "Copy if newer") and
import it:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Fonts.cs#own-font)]

Don't copy Windows fonts into your game: their license doesn't allow redistributing them. Free fonts under the SIL Open
Font License, such as Noto Sans or Inter, can be shipped with any game. When a font can't be found, IcyUI writes a
warning to Stride's log.

## Troubleshooting

- **The project was created with Stride 4.3 or earlier.** IcyUI targets .NET 10. Upgrade the project to Stride 4.4
  from the launcher, or set `<TargetFramework>net10.0</TargetFramework>` in every project file of the solution (the
  Windows platform project may use `net10.0-windows`; keep its suffix).
- **Game Studio doesn't start on Windows on ARM64** ("no compatible MSBuild found"). Use the code-only setup there,
  or edit the project in Game Studio on an x64 machine.
- **Text doesn't appear.** Check the game's log for an IcyUI warning about a missing font; see
  [Fonts on other platforms](#fonts-on-other-platforms).

## Next

[Your first screen](first-screen.md) replaces the button with a menu described in markup.
````

`docfx/en/getting-started/monogame.md` — replace steps 4–5 with:
````markdown
4. **Set IcyUI up in your `Game` class.** Call `UseIcyUI()` in `Initialize()`:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#game)]

   `UseIcyUI()` adds IcyUI as a game component and returns the host: `ui.MainCanvas` is the canvas your elements go
   on, and `ui.Configuration` holds every setting. The defaults load the built-in theme and use the system font
   Segoe UI. IcyUI draws during `base.Draw()`, after your own drawing; anything you draw after `base.Draw()` covers
   the UI. A string used as content becomes a <xref:Icy.UI.Controls.TextBlock>.

5. **Run the game.** A "Hello, IcyUI" button appears and reacts to the mouse and keyboard.

> [!NOTE]
> IcyUI doesn't ship a font. On Linux and macOS, or to use your own font, import a font file shipped with the game:
> `this.UseIcyUI(ui => ui.ConfigureFonts().ImportFont("Fonts/MyFont.ttf").WithDefaultFontFamily("My Font"))`. See
> [Fonts on other platforms](stride.md#fonts-on-other-platforms). IcyUI's warnings appear in the debugger's Output
> window.
````
(Keep steps 1–3 and the existing DesktopGL note; step 2's retarget stays: the 3.8.5.1 templates target `net9.0`.)

`docfx/en/getting-started/first-screen.md` — replace the section "## 2. Load it and give it data" through the `---`
line and the paragraph after it with:
````markdown
## 2. Load it and give it data

# [Stride](#tab/stride)

Replace your startup script with this one (and pick `MenuStartup` in **Add component**):

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#first-screen)]

# [MonoGame](#tab/monogame)

Load the menu at the end of `Initialize()`:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#first-screen)]

---

<xref:Icy.UI.Canvas.LoadMarkup(System.String,System.Object)> loads the markup file from the game's output folder,
sets the view model as the menu's <xref:Icy.UI.UIElement.DataContext> so every `{Binding}` inside reads from it, and
adds the menu to the canvas. Clicking a button, or activating it with the keyboard or a gamepad, runs its command.
````
and in "What you built" keep the text. In "Next", keep both bullets.

`docfx/en/markup/index.md` — replace the "### Text content" section's last paragraph ("For `Button`, whose content …
never renders invisibly.") with:
```markdown
For `Button`, whose content is an element rather than a string, the text is wrapped in a `TextBlock` automatically.
The same happens to a string assigned as an attribute or through a binding, anywhere an element is expected:

```xml
<Button Content="Play"/>
<Button Content="{Binding Title}"/>
```

A binding creates a new `TextBlock` each time its value changes; for text that changes often, bind the text itself:
`<Button><TextBlock Text="{Binding Score}"/></Button>`. Text without a `FontFamily` uses the configuration's default
family (Segoe UI with the engine defaults); when no font resolves, IcyUI logs a warning.
```

- [ ] **Step 4: Russian pages**

Mirror each English change in `docfx/ru/…`, following `_glossary.md` (code stays English). Exact replacements:

`docfx/ru/getting-started/stride.md` — whole page (keep the front matter, the hash is set in Step 5):
````markdown
# Начало работы со Stride

На этой странице мы выведем кнопку IcyUI в игре на Stride. Большинство игр на Stride — это проекты Game Studio,
поэтому этот путь идёт первым; если игра написана только кодом, переходите к разделу
[Без Game Studio](#без-game-studio).

## В проекте Game Studio

1. **Создайте проект** в лаунчере Stride (Stride 4.4 или новее; подойдёт любой шаблон, например «New Game»).

2. **Добавьте пакет** в проект игры `MyGame/MyGame.csproj` (общий, а не проект платформы):

   ```
   dotnet add MyGame/MyGame.csproj package IcyUI.Stride --prerelease
   ```

3. **Добавьте стартовый скрипт.** Создайте в проекте игры файл `IcyUIStartup.cs`:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#startup-script)]

   `UseIcyUI()` подключает IcyUI к игре и рисует интерфейс поверх всего, что рисует сцена. Метод возвращает хост:
   `ui.MainCanvas` — это Canvas, на который добавляются элементы, а `ui.Configuration` хранит все настройки. Настройки
   по умолчанию загружают встроенную тему и используют системный шрифт Segoe UI. Строка, переданная как содержимое,
   становится <xref:Icy.UI.Controls.TextBlock>.

4. **Прикрепите скрипт.** Снова откройте проект в Game Studio и дождитесь перезагрузки скриптов. В стартовой сцене
   выберите любую сущность (подойдёт камера), затем в панели свойств нажмите **Add component** и выберите
   `IcyUIStartup`.

5. **Запустите игру.** Поверх сцены появится кнопка «Hello, IcyUI», реагирующая на мышь и клавиатуру.

Другие скрипты получают тот же интерфейс через `Game.GetIcyCanvas()` или `Game.GetIcyHost()`.

## Без Game Studio

Если вы наследуете собственный класс `Game` и настраиваете сцену в коде, подключите IcyUI в `BeginRun()`, когда
графический компоновщик уже создан:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#code-only)]

`UseIcyUI()` рисует через `SceneSystem.GraphicsCompositor`, поэтому к этому моменту он не должен быть `null`.

## Шрифты на других платформах

IcyUI не поставляется со шрифтом: по умолчанию используется Segoe UI из папки шрифтов Windows. На Linux и macOS или
чтобы везде использовать свой шрифт, положите файл шрифта в игру (свойство **Copy to Output Directory** — «Copy if
newer») и импортируйте его:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Fonts.cs#own-font)]

Не копируйте шрифты Windows в игру: их лицензия не разрешает распространение. Свободные шрифты под лицензией SIL Open
Font License, например Noto Sans или Inter, можно поставлять с любой игрой. Если шрифт не найден, IcyUI пишет
предупреждение в журнал Stride.

## Устранение неполадок

- **Проект создан в Stride 4.3 или старше.** IcyUI нацелен на .NET 10. Обновите проект до Stride 4.4 в лаунчере или
  укажите `<TargetFramework>net10.0</TargetFramework>` в каждом файле проекта решения (проект платформы Windows может
  использовать `net10.0-windows` — сохраните его суффикс).
- **Game Studio не запускается на Windows для ARM64** («no compatible MSBuild found»). Используйте там настройку
  без Game Studio или редактируйте проект в Game Studio на компьютере x64.
- **Текст не появляется.** Найдите в журнале игры предупреждение IcyUI об отсутствующем шрифте; см.
  [Шрифты на других платформах](#шрифты-на-других-платформах).

## Далее

На странице [Ваш первый экран](first-screen.md) кнопка заменяется меню, описанным разметкой.
````

`docfx/ru/getting-started/monogame.md` — replace steps 4–5 with:
````markdown
4. **Настройте IcyUI в классе `Game`.** Вызовите `UseIcyUI()` в `Initialize()`:

   [!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#game)]

   `UseIcyUI()` добавляет IcyUI как компонент игры и возвращает хост: `ui.MainCanvas` — это Canvas, на который
   добавляются элементы, а `ui.Configuration` хранит все настройки. Настройки по умолчанию загружают встроенную тему и
   используют системный шрифт Segoe UI. IcyUI рисует во время `base.Draw()`, после вашей отрисовки; всё, что
   нарисовано после `base.Draw()`, перекроет интерфейс. Строка, переданная как содержимое, становится
   <xref:Icy.UI.Controls.TextBlock>.

5. **Запустите игру.** Появится кнопка «Hello, IcyUI», реагирующая на мышь и клавиатуру.

> [!NOTE]
> IcyUI не поставляется со шрифтом. На Linux и macOS или чтобы использовать свой шрифт, импортируйте файл шрифта,
> поставляемый с игрой:
> `this.UseIcyUI(ui => ui.ConfigureFonts().ImportFont("Fonts/MyFont.ttf").WithDefaultFontFamily("My Font"))`. См.
> [Шрифты на других платформах](stride.md#шрифты-на-других-платформах). Предупреждения IcyUI появляются в окне
> Output отладчика.
````

`docfx/ru/getting-started/first-screen.md` — replace "## 2. Загрузите её и передайте данные" through the paragraph
after `---` with:
````markdown
## 2. Загрузите её и передайте данные

# [Stride](#tab/stride)

Замените стартовый скрипт этим (и выберите `MenuStartup` в **Add component**):

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/GettingStarted.cs#first-screen)]

# [MonoGame](#tab/monogame)

Загрузите меню в конце `Initialize()`:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.MonoGame/GettingStarted.cs#first-screen)]

---

<xref:Icy.UI.Canvas.LoadMarkup(System.String,System.Object)> загружает файл разметки из папки сборки игры, делает
модель представления <xref:Icy.UI.UIElement.DataContext> меню, чтобы каждая `{Binding}` внутри читала данные из неё,
и добавляет меню на Canvas. Щелчок по кнопке или её активация с клавиатуры или геймпада выполняет её команду.
````

`docfx/ru/markup/index.md` — replace "У `Button` содержимое — элемент … никогда не становится невидимым." with:
````markdown
У `Button` содержимое — элемент, а не строка, поэтому текст автоматически оборачивается в `TextBlock`. То же
происходит со строкой, заданной атрибутом или через привязку, везде, где ожидается элемент:

```xml
<Button Content="Play"/>
<Button Content="{Binding Title}"/>
```

Привязка создаёт новый `TextBlock` при каждом изменении значения; для часто меняющегося текста привязывайте сам
текст: `<Button><TextBlock Text="{Binding Score}"/></Button>`. Текст без `FontFamily` использует шрифт по умолчанию из
конфигурации (Segoe UI с настройками движков по умолчанию); если шрифт не найден, IcyUI пишет предупреждение в журнал.
````
Add to `docfx/ru/_glossary.md`'s table: `| host | хост | `IIcyHost` в коде |`, `| log | журнал | |`,
`| troubleshooting | устранение неполадок | |`.

- [ ] **Step 5: Re-hash the Russian pages**

For each of `getting-started/stride.md`, `getting-started/monogame.md`, `getting-started/first-screen.md`,
`markup/index.md`, set the Russian page's `source-hash:` to the output of
`tr -d '\r' < docfx/en/<page> | sha256sum | cut -c1-12`.

- [ ] **Step 6: READMEs and CLAUDE.md**

`sources/IcyUI.MonoGame/README.md` code block:
```csharp
using Icy.MonoGame.Configuration;
using Icy.UI.Controls;

// In Game.Initialize(), before base.Initialize():
MonoGameIcyHost ui = this.UseIcyUI();
ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });
// IcyUI draws itself during base.Draw().
```
`sources/IcyUI.Stride/README.md` code block:
```csharp
using Icy.Stride.Configuration;
using Icy.UI.Controls;

// In a startup script's Start() (or Game.BeginRun() after the scene system is set up):
StrideIcyHost ui = Game.UseIcyUI();
ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });
```
`CLAUDE.md`: in "Layout", the `IcyUI.MonoGame`/`IcyUI.Stride` rows mention that `UseIcyUI()` returns the engine's
`IIcyHost` (MonoGame: `MonoGameIcyHost`, a `DrawableGameComponent` that draws `Canvases`; Stride: `StrideIcyHost`, a
game system, with `IcyUISceneRenderer` drawing). Add a "Conventions" bullet: "IcyUI logs through
`IcyConfiguration.Logging` (`Microsoft.Extensions.Logging.Abstractions`). New messages go in `Diagnostics/Logging/IcyLog.cs`
as `[LoggerMessage]` methods (ids: 1xxx fonts, 2xxx bindings, 3xxx types); log once per cause, never per frame."

- [ ] **Step 7: Build the docs and check translations**

Run:
```
dotnet build sources/IcyUI.Docs.Snippets.Stride/IcyUI.Docs.Snippets.Stride.csproj -c Release
dotnet build sources/IcyUI.Docs.Snippets.MonoGame/IcyUI.Docs.Snippets.MonoGame.csproj -c Release
bash .github/scripts/build-docs.sh > /tmp/docs.log 2>&1; echo exit=$?
bash .github/scripts/check-translations.sh
```
Expected: `exit=0`; the translation check reports every page fresh, none missing.

- [ ] **Step 8: Commit**

```bash
git add sources/IcyUI.Docs.Snippets.Stride sources/IcyUI.Docs.Snippets.MonoGame docfx sources/IcyUI.MonoGame/README.md sources/IcyUI.Stride/README.md CLAUDE.md
git commit -m "Docs: Getting started on the host API, fonts on other platforms, Stride troubleshooting"
```

---

### Task 10: Whole-solution verification

- [ ] **Step 1: Release build**

Run: `dotnet build sources/IcyUI.sln -c Release > /tmp/rel.log 2>&1; grep -E "warning|error" /tmp/rel.log | grep -v NU190 | sort -u | head`
Expected: no lines (no warnings or errors apart from audit warnings).

- [ ] **Step 2: Tests**

Run: `dotnet test sources/IcyUI.Tests/IcyUI.Tests.csproj > /tmp/all.log 2>&1; tail -5 /tmp/all.log`
Expected: `Failed: 0`, Total 1874.

- [ ] **Step 3: Pack**

Run: `rm -rf artifacts && dotnet pack sources/IcyUI.sln -c Release -o artifacts && ls artifacts`
Expected: 4 `.nupkg` + 4 `.snupkg` at `0.1.0-alpha.1`; `IcyUI.Stride`'s nuspec depends on Stride `4.4.0`, `IcyUI`'s on
`Microsoft.Extensions.Logging.Abstractions`:
`unzip -p artifacts/IcyUI.Stride.0.1.0-alpha.1.nupkg IcyUI.Stride.nuspec | grep -i stride.engine` and
`unzip -p artifacts/IcyUI.0.1.0-alpha.1.nupkg IcyUI.nuspec | grep -i logging`.

- [ ] **Step 4: Hand-off**

Report to Ivan: the smoke tests that remain his (both sample hosts on x64 and ARM64; MonoGame on WindowsDX and
DesktopGL; a new Stride 4.4 Game Studio project following the Stride page with the CI `packages` artifact through a
local feed; F1/F2 debug tools and text in the samples), and the alpha.1 tag steps from `CLAUDE.md`.
