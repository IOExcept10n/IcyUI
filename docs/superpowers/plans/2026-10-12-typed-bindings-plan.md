# Typed Markup Bindings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `{Binding}` in markup builds a static (typed) path when its source type is known, a compiled path with
`Compiled=True`, and a dynamic path otherwise, with misspelled paths failing at load.

**Architecture:** The loader exposes the `x:DataType` in effect on `MarkupExtensionContext.DataType` by walking the
`XElement` ancestors up to a template boundary. `BindingExtension` picks the source type (`Source`, `ElementName`, or
`x:DataType`) and asks `ReflectionConfiguration` for a cached typed path. A `DataContext` binding wraps that path in a
`CheckedPropertyPath`, which checks the run-time type and falls back (static) or throws (compiled). `Binding` itself
is unchanged.

**Tech Stack:** C# / .NET 10, xUnit, `Microsoft.Extensions.Logging` source-generated messages.

**Spec:** `docs/superpowers/specs/2026-10-12-typed-bindings-design.md`

## Global Constraints

- Core project `sources/IcyUI` only; no engine project changes.
- Zero warnings in shipped libraries (StyleCop is on; Release makes warnings errors). Members in StyleCop order:
  fields, constructors, events, properties, methods; public before internal before private.
- Block-scoped `namespace X { }`, the two-line copyright header on every new `.cs` file in `sources/IcyUI`.
- Complete XML docs on every public member (`<see cref>`, `<see langword>`, `<list>`, `<para>`).
- New log message: `IcyLog` `[LoggerMessage]`, event id **2002**, logged once per binding, never per frame.
- Line endings: keep each file's existing endings; new files LF (git normalizes).
- Run tests: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj"`; always check the "Total" count, not only "Passed!".

## Spec deviation

- §1 says `x:DataType` is "read before the element's other attributes". Reading the attribute straight off the
  `XElement` ancestors (Task 1) makes attribute order irrelevant, so the loader's attribute loop doesn't change.

## Review Focus

- `x:DataType` on the same element as the `{Binding}` (not only on an ancestor): must apply. Pinned in Task 1.
- A `{Binding}` with no path (`{Binding}`, the source itself) under `x:DataType`, with or without `Compiled=True`:
  must keep working as today (dynamic, no load error). Pinned in Task 3.
- A `DataContext` that's a subclass of `x:DataType`, or implements the interface it names: must use the typed path, no
  warning. Pinned in Task 2.
- A data context that changes from a wrong type back to the right one: the static binding keeps working (it stays on the
  dynamic fallback; still correct values). Pinned in Task 2.
- Two-way text conversion (TextBox to a `float`) through static and compiled paths: must still convert and parse with
  the binding's culture, since `Binding` asks the path for `GetPropertyType`. Pinned in Task 3.

---

### Task 1: `MarkupExtensionContext.DataType` from `x:DataType`

**Files:**
- Modify: `sources/IcyUI/Markup/MarkupExtensionContext.cs`
- Modify: `sources/IcyUI/Markup/MarkupLoader.cs` (`ResolveExtension`, ~line 1223; new private helpers; new field)
- Modify: `sources/IcyUI/Markup/MarkupDirectives.cs` (`DataType` remarks)
- Test: `sources/IcyUI.Tests/Markup/DataTypeScopeTests.cs` (create)

**Interfaces:**
- Produces: `public Type? MarkupExtensionContext.DataType { get; }`, an optional last constructor parameter
  `Type? dataType = null`.

- [ ] **Step 1: Write the failing tests**

Create `sources/IcyUI.Tests/Markup/DataTypeScopeTests.cs`:

```csharp
using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Markup
{
    // Top-level, not nested in the test class: a clr-namespace prefix can't name a nested type (Outer+Inner).
    public sealed class ScopeOuterModel
    {
    }

    public sealed class ScopeItemModel
    {
    }

    /// <summary>Records the DataType of every context it's resolved in.</summary>
    public sealed class CaptureDataTypeExtension : IMarkupExtension
    {
        public static List<Type?> Seen { get; } = [];

        public object? ProvideValue(MarkupExtensionContext context)
        {
            Seen.Add(context.DataType);
            return MarkupValue.Unset;
        }
    }

    /// <summary>
    /// Which x:DataType a markup extension sees: the nearest one on the element or its ancestors, never one from
    /// outside the template the extension is in.
    /// </summary>
    public class DataTypeScopeTests
    {
        private static readonly string Ns =
            $"xmlns:m=\"clr-namespace:{typeof(ScopeOuterModel).Namespace};assembly={typeof(ScopeOuterModel).Assembly.GetName().Name}\"";

        private static UIElement Load(string body)
        {
            var configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
            configuration.Types.Markup.RegisterExtension<CaptureDataTypeExtension>("Capture");
            CaptureDataTypeExtension.Seen.Clear();
            return new MarkupLoader(configuration).Load(body.Replace("NS", Ns, StringComparison.Ordinal), "x.xml");
        }

        [Fact]
        public void NoDataType_IsNull()
        {
            Load("<StackPanel NS><TextBlock Text=\"{Capture}\"/></StackPanel>");

            Assert.Equal([null], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void AncestorDataType_Applies()
        {
            Load("<StackPanel NS x:DataType=\"m:ScopeOuterModel\"><Border><TextBlock Text=\"{Capture}\"/></Border></StackPanel>");

            Assert.Equal([typeof(ScopeOuterModel)], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void DataTypeOnTheSameElement_Applies_WhateverTheAttributeOrder()
        {
            Load("<StackPanel NS><TextBlock Text=\"{Capture}\" x:DataType=\"m:ScopeOuterModel\"/></StackPanel>");

            Assert.Equal([typeof(ScopeOuterModel)], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void NestedDataType_WinsOverTheOuterOne()
        {
            Load("<StackPanel NS x:DataType=\"m:ScopeOuterModel\"><Border x:DataType=\"m:ScopeItemModel\"><TextBlock Text=\"{Capture}\"/></Border></StackPanel>");

            Assert.Equal([typeof(ScopeItemModel)], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void DataTemplate_DoesNotSeeTheOuterDataType()
        {
            var root = (ItemsControl)Load(
                """
                <ItemsControl NS x:DataType="m:ScopeOuterModel">
                  <ItemsControl.ItemTemplate>
                    <DataTemplate><TextBlock Text="{Capture}"/></DataTemplate>
                  </ItemsControl.ItemTemplate>
                </ItemsControl>
                """);
            CaptureDataTypeExtension.Seen.Clear();

            root.ItemTemplate!.Build(new ScopeItemModel());

            Assert.Equal([null], CaptureDataTypeExtension.Seen);
        }

        [Fact]
        public void DataTemplate_DeclaresItsOwnDataType()
        {
            var root = (ItemsControl)Load(
                """
                <ItemsControl NS x:DataType="m:ScopeOuterModel">
                  <ItemsControl.ItemTemplate>
                    <DataTemplate x:DataType="m:ScopeItemModel"><TextBlock Text="{Capture}"/></DataTemplate>
                  </ItemsControl.ItemTemplate>
                </ItemsControl>
                """);
            CaptureDataTypeExtension.Seen.Clear();

            root.ItemTemplate!.Build(new ScopeItemModel());
            root.ItemTemplate!.Build(new ScopeItemModel());

            Assert.Equal([typeof(ScopeItemModel), typeof(ScopeItemModel)], CaptureDataTypeExtension.Seen);
        }
    }
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj" --filter "FullyQualifiedName~DataTypeScopeTests"`
Expected: compile error `'MarkupExtensionContext' does not contain a definition for 'DataType'`.

- [ ] **Step 3: Add the property**

In `MarkupExtensionContext.cs`, add a constructor parameter after `templatedControl` and its doc line:

```csharp
    /// <param name="dataType">
    /// The <c>x:DataType</c> in effect where the extension is used, or <see langword="null"/> when none is declared.
    /// </param>
    public sealed class MarkupExtensionContext(
        object target,
        MarkupMember member,
        IcyConfiguration configuration,
        MarkupNameScope names,
        IReadOnlyList<UIElement> elementStack,
        IXmlLineInfo? node,
        string? sourcePath,
        UIElement? templatedControl = null,
        Type? dataType = null)
```

and, after `TemplatedControl`:

```csharp
        /// <summary>
        /// Gets the type the data context is declared to have where the extension is used: the <c>x:DataType</c> of
        /// the nearest element, the target's own included, or <see langword="null"/> when none is declared.
        /// </summary>
        /// <remarks>
        /// A template (<see cref="Icy.UI.Styles.DataTemplate"/>, <see cref="Icy.UI.Styles.ControlTemplate"/>) is a
        /// boundary: its content sees an <c>x:DataType</c> declared on the template element or inside it, never one
        /// from outside, since its items have a data context of their own.
        /// </remarks>
        public Type? DataType { get; } = dataType;
```

- [ ] **Step 4: Resolve it in the loader**

In `MarkupLoader.cs` add `using System.Runtime.CompilerServices;` if missing, a field next to `types`:

```csharp
        // x:DataType per element, resolved once: a template built for many items walks the same XElements each time.
        private readonly ConditionalWeakTable<XElement, StrongBox<Type?>> declaredDataTypes = new();
        private readonly ConditionalWeakTable<XElement, StrongBox<bool>> templateBoundaries = new();
```

In `ResolveExtension`, pass the data type:

```csharp
            var extensionContext = new MarkupExtensionContext(instance, member, configuration, context.Names, context.ElementStack, node, context.SourcePath, context.TemplatedControl, ResolveDataType(node, context));
```

and add the helpers after `ResolveExtension`:

```csharp
        /// <summary>
        /// Finds the <c>x:DataType</c> in effect at <paramref name="node"/>: the nearest one on its element or the
        /// element's ancestors, stopping at a template boundary (the template element itself still counts).
        /// </summary>
        private Type? ResolveDataType(IXmlLineInfo? node, MarkupLoadContext context)
        {
            for (XElement? element = node as XElement ?? (node as XAttribute)?.Parent; element != null; element = element.Parent)
            {
                if (element.Attribute(MarkupNamespaces.DirectivesNamespace + MarkupDirectives.DataType) is { } declared)
                {
                    return declaredDataTypes.GetValue(
                        element,
                        e => new StrongBox<Type?>(types.ResolveTypeName(declared.Value, e, declared, context.SourcePath))).Value;
                }

                if (IsTemplateBoundary(element, context))
                    return null;
            }

            return null;
        }

        // A DataTemplate (HierarchicalDataTemplate included) or ControlTemplate element. Property elements
        // (Type.Member) are never boundaries.
        private bool IsTemplateBoundary(XElement element, MarkupLoadContext context) =>
            !element.Name.LocalName.Contains('.', StringComparison.Ordinal)
            && templateBoundaries.GetValue(element, e =>
            {
                Type type = types.Resolve(e.Name, e, context.SourcePath);
                return new StrongBox<bool>(typeof(Icy.UI.Styles.DataTemplate).IsAssignableFrom(type) || type == typeof(Icy.UI.Styles.ControlTemplate));
            }).Value;
```

Update the `MarkupDirectives.DataType` remarks to: "Resolved at load so a misspelling is caught. `{Binding}` uses it
as the type of the data context: see `Icy.Markup.Extensions.BindingExtension`." Update `MarkupLoadContext.DataTypes`
remarks to say it validates the directive; the run-time lookup reads the attribute (see `ResolveDataType`).

- [ ] **Step 5: Run the tests**

Run: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj" --filter "FullyQualifiedName~DataTypeScopeTests"`
Expected: 6 passed. Then the full suite: all pass, Total = previous + 6.

- [ ] **Step 6: Commit**

```bash
git add sources/IcyUI/Markup sources/IcyUI.Tests/Markup/DataTypeScopeTests.cs
git commit -m "Markup extensions see the x:DataType in effect, scoped by templates"
```

---

### Task 2: Typed-path cache and `CheckedPropertyPath`

**Files:**
- Modify: `sources/IcyUI/Configuration/ReflectionConfiguration.cs`
- Create: `sources/IcyUI/Data/Bindings/CheckedPropertyPath.cs`
- Modify: `sources/IcyUI/Diagnostics/Logging/IcyLog.cs`
- Test: `sources/IcyUI.Tests/Data/Bindings/CheckedPropertyPathTests.cs` (create)

**Interfaces:**
- Produces: `internal PropertyPath ReflectionConfiguration.GetTypedPath(Type sourceType, string path, bool compiled)`;
  `internal sealed class CheckedPropertyPath(PropertyPath typed, Type declaredType, bool compiled, string path, ILogger? logger) : IPropertyPath`;
  `IcyLog.BindingSourceTypeMismatch(ILogger logger, string path, string expectedType, string actualType)` (2002).

- [ ] **Step 1: Write the failing tests**

```csharp
using Icy.Configuration;
using Icy.Data.Bindings;
using Icy.Tests.Logging;

namespace Icy.Tests.Data.Bindings
{
    public class CheckedPropertyPathTests
    {
        public interface INamed
        {
            string Name { get; }
        }

        public class Hero : INamed
        {
            public string Name { get; set; } = "Ann";
        }

        public sealed class Paladin : Hero
        {
        }

        public sealed class Villain
        {
            public string Name { get; set; } = "Zed";
        }

        private static (CheckedPropertyPath Path, CapturingLoggerFactory Log) Create(Type declared, bool compiled)
        {
            var log = new CapturingLoggerFactory();
            PropertyPath typed = new ReflectionConfiguration().GetTypedPath(declared, "Name", compiled);
            return (new CheckedPropertyPath(typed, declared, compiled, "Name", log.CreateLogger("test")), log);
        }

        [Fact]
        public void GetTypedPath_IsSharedPerTypePathAndKind()
        {
            var types = new ReflectionConfiguration();

            Assert.Same(types.GetTypedPath(typeof(Hero), "Name", compiled: true), types.GetTypedPath(typeof(Hero), "Name", compiled: true));
            Assert.NotSame(types.GetTypedPath(typeof(Hero), "Name", compiled: true), types.GetTypedPath(typeof(Hero), "Name", compiled: false));
            Assert.IsType<CompiledPropertyPath>(types.GetTypedPath(typeof(Hero), "Name", compiled: true));
            Assert.IsNotType<CompiledPropertyPath>(types.GetTypedPath(typeof(Hero), "Name", compiled: false));
        }

        [Fact]
        public void GetTypedPath_MissingMember_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ReflectionConfiguration().GetTypedPath(typeof(Hero), "Nmae", compiled: false));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SubclassAndInterface_UseTheTypedPath_WithoutWarning(bool compiled)
        {
            var (path, log) = Create(typeof(INamed), compiled);

            Assert.Equal("Ann", path.GetValue(new Paladin()));
            Assert.Empty(log.Entries);
        }

        [Fact]
        public void Static_WrongType_WarnsOnceAndReadsDynamically()
        {
            var (path, log) = Create(typeof(Hero), compiled: false);

            Assert.Equal("Zed", path.GetValue(new Villain()));
            Assert.Equal("Zed", path.GetValue(new Villain()));
            Assert.Equal("Ann", path.GetValue(new Hero()));

            CapturedLog entry = Assert.Single(log.Entries, e => e.EventId.Id == 2002);
            Assert.Contains(nameof(Hero), entry.Message);
            Assert.Contains(nameof(Villain), entry.Message);
        }

        [Fact]
        public void Compiled_WrongType_Throws()
        {
            var (path, _) = Create(typeof(Hero), compiled: true);

            var ex = Assert.Throws<BindingException>(() => path.GetValue(new Villain()));
            Assert.Contains(nameof(Hero), ex.Message);
            Assert.Contains(nameof(Villain), ex.Message);
        }

        [Fact]
        public void SetValue_GoesThroughTheTypedPath()
        {
            var (path, _) = Create(typeof(Hero), compiled: true);
            var hero = new Hero();

            path.SetValue(hero, "Bea");

            Assert.Equal("Bea", hero.Name);
            Assert.Equal(typeof(string), path.GetPropertyType(hero));
        }
    }
}
```

`CapturingLoggerFactory` (an `ILoggerFactory`) and `CapturedLog` are in `IcyUI.Tests/Logging`, namespace
`Icy.Tests.Logging`. Nested types are fine in this file: it builds paths from `Type` objects, never through markup.

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj" --filter "FullyQualifiedName~CheckedPropertyPathTests"`
Expected: compile errors (`GetTypedPath`, `CheckedPropertyPath` don't exist).

- [ ] **Step 3: The cache**

In `ReflectionConfiguration.cs` (`using System.Collections.Concurrent; using Icy.Data.Bindings;`):

```csharp
        // Typed paths are immutable once built, so every binding on the same (registry, type, path, kind) shares one.
        // Bounded by the distinct paths in the game's markup; never evicted.
        private readonly ConcurrentDictionary<(PropertyRegistry Registry, Type Type, string Path, bool Compiled), PropertyPath> typedPaths = new();
```

after the public properties:

```csharp
        /// <summary>
        /// Gets the typed path for <paramref name="path"/> on <paramref name="sourceType"/>, building it on first use.
        /// </summary>
        /// <param name="sourceType">The type the path starts at.</param>
        /// <param name="path">The member path, such as <c>Player.Name</c>.</param>
        /// <param name="compiled">
        /// <see langword="true"/> for a <see cref="CompiledPropertyPath"/>; <see langword="false"/> for a reflection-based
        /// <see cref="PropertyPath"/>.
        /// </param>
        /// <returns>The shared path.</returns>
        /// <exception cref="ArgumentException">A member of <paramref name="path"/> doesn't exist.</exception>
        internal PropertyPath GetTypedPath(Type sourceType, string path, bool compiled) =>
            typedPaths.GetOrAdd(
                (PropertyRegistry, sourceType, path, compiled),
                static key => key.Compiled
                    ? new CompiledPropertyPath(key.Path, key.Type, key.Registry)
                    : new PropertyPath(key.Path, key.Type, key.Registry));
```

`GetOrAdd` doesn't cache a factory that throws, so a misspelled path is reported every time it's used.

- [ ] **Step 4: The log message**

In `IcyLog.cs` after 2001:

```csharp
        [LoggerMessage(EventId = 2002, Level = LogLevel.Warning, Message = "Binding path '{Path}' expects a '{ExpectedType}' (x:DataType) but its data context is a '{ActualType}'; it reads through a dynamic path from now on. Fix x:DataType or the data context.")]
        public static partial void BindingSourceTypeMismatch(ILogger logger, string path, string expectedType, string actualType);
```

Also change the class summary's id list to "1xxx fonts, 2xxx bindings, 3xxx types, 4xxx localization".

- [ ] **Step 5: `CheckedPropertyPath`**

Create `sources/IcyUI/Data/Bindings/CheckedPropertyPath.cs`:

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Diagnostics.Logging;
using Microsoft.Extensions.Logging;

namespace Icy.Data.Bindings
{
    /// <summary>
    /// A typed path for a binding whose source is a data context declared with <c>x:DataType</c>: the data context can
    /// turn out to be another type at run time, so each read and write checks it first.
    /// </summary>
    /// <param name="typed">The shared typed path, built for <paramref name="declaredType"/>.</param>
    /// <param name="declaredType">The <c>x:DataType</c>.</param>
    /// <param name="compiled">
    /// Whether the binding asked for a compiled path: a wrong type then throws instead of falling back.
    /// </param>
    /// <param name="path">The path text, for the fallback and the messages.</param>
    /// <param name="logger">Where the fallback is reported, or <see langword="null"/>.</param>
    /// <remarks>
    /// A source of the declared type, a subclass or an implementation goes through <paramref name="typed"/>. Any other
    /// source, for a static path, logs <see cref="IcyLog.BindingSourceTypeMismatch"/> once and reads through a
    /// <see cref="DynamicPropertyPath"/> from then on; for a compiled path, it throws <see cref="BindingException"/>.
    /// </remarks>
    internal sealed class CheckedPropertyPath(PropertyPath typed, Type declaredType, bool compiled, string path, ILogger? logger) : IPropertyPath
    {
        private DynamicPropertyPath? fallback;

        /// <inheritdoc/>
        public Type PropertyType => typed.PropertyType;

        /// <inheritdoc/>
        public object? GetValue(object source) => For(source).GetValue(source);

        /// <inheritdoc/>
        public void SetValue(object target, object? value) => For(target).SetValue(target, value);

        /// <inheritdoc/>
        public Type GetPropertyType(object source) => For(source).GetPropertyType(source);

        /// <inheritdoc/>
        public bool DependsOn(string propertyName) => typed.DependsOn(propertyName);

        private IPropertyPath For(object source)
        {
            if (fallback != null)
                return fallback;

            if (declaredType.IsInstanceOfType(source))
                return typed;

            if (compiled)
            {
                throw new BindingException(
                    $"The compiled binding path '{path}' expects a '{declaredType.Name}' (x:DataType) but its data context is a '{source.GetType().Name}'. Fix x:DataType or the data context.");
            }

            if (logger != null)
                IcyLog.BindingSourceTypeMismatch(logger, path, declaredType.Name, source.GetType().Name);
            return fallback = new DynamicPropertyPath(path, logger);
        }
    }
}
```

`PropertyPath` implements `IPropertyPath`, and `IReadOnlyPropertyPath` gives `GetPropertyType` and `DependsOn` default
implementations, so the delegating calls compile whether or not `PropertyPath` overrides them.

- [ ] **Step 6: Run the tests**

Run: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj" --filter "FullyQualifiedName~CheckedPropertyPathTests"`
Expected: 8 passed. Build `sources/IcyUI` with no warnings.

- [ ] **Step 7: Commit**

```bash
git add sources/IcyUI sources/IcyUI.Tests/Data/Bindings/CheckedPropertyPathTests.cs
git commit -m "Shared typed binding paths, and a run-time type check for x:DataType bindings"
```

---

### Task 3: `BindingExtension` picks the path

**Files:**
- Modify: `sources/IcyUI/Markup/Extensions/BindingExtension.cs`
- Test: `sources/IcyUI.Tests/Markup/TypedBindingTests.cs` (create)

**Interfaces:**
- Consumes: `MarkupExtensionContext.DataType` (Task 1); `ReflectionConfiguration.GetTypedPath`, `CheckedPropertyPath`
  (Task 2).
- Produces: `public bool BindingExtension.Compiled { get; set; }`.

- [ ] **Step 1: Write the failing tests**

```csharp
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Assets;
using Icy.Configuration;
using Icy.Data.Bindings;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Markup
{
    public sealed class MenuModel : ObservableObject
    {
        private string title = "Play";
        private float volume = 0.5f;

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public float Volume
        {
            get => volume;
            set => SetProperty(ref volume, value);
        }

        public override string ToString() => "menu";
    }

    public sealed class OtherModel
    {
        public string Title { get; set; } = "Other";
    }

    public class TypedBindingTests
    {
        private static readonly string Ns =
            $"xmlns:m=\"clr-namespace:{typeof(MenuModel).Namespace};assembly={typeof(MenuModel).Assembly.GetName().Name}\"";

        private static (UIElement Root, CapturingLoggerFactory Log) Load(string body)
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureLogging(log)
                .Build();
            configuration.Culture = CultureInfo.InvariantCulture;
            return (new MarkupLoader(configuration).Load(body.Replace("NS", Ns, StringComparison.Ordinal), "x.xml"), log);
        }

        private static IPropertyPath PathOf(UIElement element) => Assert.IsType<Binding>(Assert.Single(((IBindingTarget)element).Bindings)).Path;

        [Fact]
        public void NoDataType_StaysDynamic()
        {
            var (root, _) = Load("<TextBlock NS Text=\"{Binding Title}\"/>");

            Assert.IsType<DynamicPropertyPath>(PathOf(root));
        }

        [Fact]
        public void DataType_MakesAStaticCheckedPath()
        {
            var (root, _) = Load("<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{Binding Title}\"/>");
            root.DataContext = new MenuModel();

            Assert.IsType<CheckedPropertyPath>(PathOf(root));
            Assert.Equal("Play", ((TextBlock)root).Text);
        }

        [Fact]
        public void Compiled_UnderDataType_ReadsAndWritesBack()
        {
            var (root, _) = Load("<TextBox NS x:DataType=\"m:MenuModel\" Text=\"{Binding Volume, Mode=TwoWay, Compiled=True}\"/>");
            var model = new MenuModel();
            root.DataContext = model;

            Assert.Equal("0.5", ((TextBox)root).Text);
            ((TextBox)root).Text = "0.75";
            Assert.Equal(0.75f, model.Volume);
        }

        [Fact]
        public void MisspelledPath_UnderDataType_FailsAtLoad_WithASuggestion()
        {
            var ex = Assert.Throws<MarkupException>(() => Load("<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{Binding Titel}\"/>"));

            Assert.Contains("Titel", ex.Message);
            Assert.Contains("Title", ex.Message);
            Assert.Contains("MenuModel", ex.Message);
        }

        [Fact]
        public void MisspelledPath_WithElementName_FailsAtLoad()
        {
            var ex = Assert.Throws<MarkupException>(() => Load("<StackPanel NS><Slider x:Name=\"volume\"/><TextBlock Text=\"{Binding Valeu, ElementName=volume}\"/></StackPanel>"));

            Assert.Contains("Valeu", ex.Message);
        }

        [Fact]
        public void ElementName_GetsAStaticPathOnTheNamedElementsType()
        {
            var (root, _) = Load("<StackPanel NS><Slider x:Name=\"volume\"/><TextBlock Text=\"{Binding Value, ElementName=volume}\"/></StackPanel>");

            IPropertyPath path = PathOf(((StackPanel)root).Children[1]);
            Assert.IsAssignableFrom<PropertyPath>(path);
        }

        [Fact]
        public void Compiled_WithoutAKnownSourceType_FailsAtLoad()
        {
            var ex = Assert.Throws<MarkupException>(() => Load("<TextBlock NS Text=\"{Binding Title, Compiled=True}\"/>"));

            Assert.Contains("x:DataType", ex.Message);
        }

        [Theory]
        [InlineData("{Binding}")]
        [InlineData("{Binding Compiled=True}")]
        public void NoPath_UnderDataType_StillBindsTheWholeSource(string binding)
        {
            // MenuModel.ToString() is "menu": the whole source, formatted into the text.
            var (root, _) = Load($"<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{binding}\"/>");

            root.DataContext = new MenuModel();

            Assert.Equal("menu", ((TextBlock)root).Text);
        }

        [Fact]
        public void Static_WrongDataContext_WarnsAndStillReads()
        {
            var (root, log) = Load("<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{Binding Title}\"/>");

            root.DataContext = new OtherModel();

            Assert.Equal("Other", ((TextBlock)root).Text);
            Assert.Single(log.Entries, e => e.EventId.Id == 2002);
        }

        [Fact]
        public void Compiled_WrongDataContext_ThrowsWhereItIsSet()
        {
            var (root, _) = Load("<TextBlock NS x:DataType=\"m:MenuModel\" Text=\"{Binding Title, Compiled=True}\"/>");

            var ex = Assert.Throws<BindingException>(() => root.DataContext = new OtherModel());
            Assert.Contains("OtherModel", ex.ToString());
        }
    }
}
```

`IBindingTarget.Bindings` is an `IReadOnlyCollection<IBinding>`; `Binding.Path` is the `IPropertyPath` it was built
with. The models are top-level for the same `clr-namespace` reason as in Task 1.

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj" --filter "FullyQualifiedName~TypedBindingTests"`
Expected: the build passes (`Compiled=True` is an unknown extension argument only at run time) and most tests fail:
`DataType_MakesAStaticCheckedPath` gets a `DynamicPropertyPath`, the `Compiled=True` ones throw "unknown argument".

- [ ] **Step 3: Implement**

In `BindingExtension.cs`, add the property after `UpdateTargetTrigger`:

```csharp
        /// <summary>
        /// Gets or sets a value indicating whether the path is compiled into delegates (<see cref="CompiledPropertyPath"/>)
        /// instead of read through reflection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Needs a known source type: an <c>x:DataType</c> on this element or an ancestor for a data-context binding, or
        /// <see cref="ElementName"/> or <see cref="Source"/>. Without one, the markup fails to load.
        /// </para>
        /// <para>
        /// A compiled data-context binding throws <see cref="BindingException"/> when the data context isn't the
        /// <c>x:DataType</c>; a static one warns and falls back to a dynamic path. Under NativeAOT, compiled paths are
        /// interpreted, at about the speed of static ones.
        /// </para>
        /// </remarks>
        public bool Compiled { get; set; }
```

Replace the path construction at the top of `ProvideValue` (the `var binding = new Binding(new DynamicPropertyPath(...))`
block) and the later `ElementName` lookup with:

```csharp
            UIElement? named = null;
            if (ElementName != null && !context.Names.TryFind(ElementName, out named))
                throw MarkupException.At($"No element named '{ElementName}' is in scope.", context.Node, context.SourcePath);

            var binding = new Binding(CreatePath(context, named))
            {
                TextConverter = context.Configuration.Types.TypeConverter,
                LoadingConfiguration = context.Configuration,
            };
```

and in the source block use `binding.Source = named;` for the `ElementName` branch (keep the order `Source`, then
`ElementName`, then the data context).

Add the private methods:

```csharp
        private IPropertyPath CreatePath(MarkupExtensionContext context, UIElement? named)
        {
            ILogger logger = context.Configuration.Logging.CreateLogger<Binding>();
            if (string.IsNullOrEmpty(Path) || Path == "this")
                return new DynamicPropertyPath("this", logger);

            bool dataContextBinding = Source == null && named == null;
            Type? sourceType = Source?.GetType() ?? named?.GetType() ?? (dataContextBinding ? context.DataType : null);
            if (sourceType == null)
            {
                if (Compiled)
                {
                    throw MarkupException.At(
                        $"'{{Binding {Path}, Compiled=True}}' needs a known source type: declare x:DataType on this element or an ancestor, or bind with ElementName or Source.",
                        context.Node,
                        context.SourcePath);
                }

                return new DynamicPropertyPath(Path, logger);
            }

            PropertyPath typed;
            try
            {
                typed = context.Configuration.Types.GetTypedPath(sourceType, Path, Compiled);
            }
            catch (ArgumentException ex)
            {
                throw MarkupException.At(DescribeMissingMember(sourceType, Path), context.Node, context.SourcePath, ex);
            }

            return dataContextBinding ? new CheckedPropertyPath(typed, sourceType, Compiled, Path, logger) : typed;
        }

        // Names the first member of the path that doesn't exist, and the type it was looked for on.
        private static string DescribeMissingMember(Type sourceType, string path)
        {
            Type current = sourceType;
            foreach (string part in path.Split('.'))
            {
                string name = part.Split('[')[0];
                if (name.Length == 0 || name == "this")
                    continue;

                PropertyInfo? property = current.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    IEnumerable<string> members = current.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name);
                    return $"'{name}' isn't a public property of '{current.Name}' (binding path '{path}').{NameSuggestion.Clause(name, members)}";
                }

                current = property.PropertyType;
            }

            return $"The binding path '{path}' doesn't resolve on '{sourceType.Name}'.";
        }
```

Usings: `System.Reflection`, `Icy.Data.Bindings`, `Microsoft.Extensions.Logging` (already), `System.Linq` if implicit
usings don't cover it. Registered (non-CLR) properties can't be missing from `GetProperty` and still resolve through
the registry; the fallback message covers that rare case. Update the class remarks with one `<para>`: "A path is
static (resolved once on a known type), compiled (see `Compiled`), or dynamic (resolved on each read) when the source
type is unknown; see the Data binding concept page."

- [ ] **Step 4: Run the tests**

Run: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj" --filter "FullyQualifiedName~TypedBindingTests"`
Expected: 11 passed. Then the full suite: all pass. Existing `{Binding ..., ElementName=...}` tests now get a static
path: if one fails because an element's runtime type lacks a member it bound to dynamically, that test documents a
behaviour change; report it rather than editing the test.

- [ ] **Step 5: Commit**

```bash
git add sources/IcyUI/Markup/Extensions/BindingExtension.cs sources/IcyUI.Tests/Markup/TypedBindingTests.cs
git commit -m "{Binding} builds static paths for known source types, compiled ones with Compiled=True"
```

---

### Task 4: Docs

**Files:**
- Modify: `docfx/en/concepts/data-binding.md`
- Create: `docfx/snippets/markup/concepts/typed-binding.xml`
- Modify: `sources/IcyUI.Tests/Docs/DocsMarkupSnippetTests.cs`
- Modify: `docfx/en/markup/index.md` (the `{Binding}` and directives sections)
- Modify: `sources/IcyUI.Docs.Snippets.Stride/...` only if a C# example is needed (none planned).

- [ ] **Step 1: Snippet and its test**

`docfx/snippets/markup/concepts/typed-binding.xml`. The view model is `Icy.Tests.Docs.DocsSnippetViewModel` (it has
`Title`, default "My Game"), the one `FirstScreen_TitleFollowsTheViewModel` uses:

```xml
<!-- x:DataType declares the data context's type: paths are checked when the page loads. -->
<StackPanel Orientation="Vertical" x:DataType="docs:DocsSnippetViewModel"
            xmlns:docs="clr-namespace:Icy.Tests.Docs;assembly=IcyUI.Tests">
  <TextBlock Text="{Binding Title}" />
  <TextBlock Text="{Binding Title, Compiled=True}" />
</StackPanel>
```

The page's prose calls the type "your view model". Add to `DocsMarkupSnippetTests`:

```csharp
        [Fact]
        public void TypedBinding_FollowsTheViewModel()
        {
            var root = (StackPanel)Load(Path.Combine("concepts", "typed-binding.xml"));
            var viewModel = new DocsSnippetViewModel();
            root.DataContext = viewModel;

            viewModel.Title = "Paused";

            Assert.All(root.Children, c => Assert.Equal("Paused", Assert.IsType<TextBlock>(c).Text));
        }
```

- [ ] **Step 2: Concept page section**

In `data-binding.md`, add after the "Conversion" section:

```markdown
## Binding kinds: dynamic, static, compiled

A binding reads its path one of three ways:

| Kind | When | Misspelled path | Reads |
|---|---|---|---|
| Dynamic | The source type isn't known: no `x:DataType`, no `ElementName` or `Source`. | A warning (event 2001) on first read, and `null`. | Looks the members up on each read. |
| Static | The source type is known. The default then. | The page fails to load, naming the line. | Through members found once. |
| Compiled | `Compiled=True`, with a known source type. | The page fails to load. | Through compiled code: the fastest. |

Declare the data context's type with `x:DataType`; every `{Binding}` below it is static, and `Compiled=True` makes one
compiled:

[!code-xml[](../../snippets/markup/concepts/typed-binding.xml)]

- `x:DataType` applies to the element and everything inside it, except templates: a `DataTemplate` declares the type of
  its items with an `x:DataType` of its own.
- `ElementName=` and `Source=` bindings are static without `x:DataType`: their source is known when the page loads.
- If the data context turns out to be another type at run time, a static binding logs a warning (event 2002) and keeps
  working dynamically; a compiled one throws, since you asked for it on purpose. A subclass of the declared type is
  fine.
- Compiling a path costs some time the first time a page uses it; every binding on the same path shares it. In a
  NativeAOT build, compiled paths are interpreted and run at about the speed of static ones.
```

Add 2002 to the event table in `logging-and-diagnostics.md` ("A binding's data context isn't its `x:DataType`; it falls
back to a dynamic path.").

- [ ] **Step 3: Markup reference**

In `docfx/en/markup/index.md`, under the `{Binding}` arguments, add `Compiled` ("`True` for a compiled path; needs a
known source type"), and under the directives, describe `x:DataType` (scope, template boundary, link to the concept
section).

- [ ] **Step 4: Build docs and run tests**

Run: `bash .github/scripts/build-docs.sh` (expected: 0 warnings) and the full test suite.
Run: `bash .github/scripts/check-translations.sh` (Russian concept pages are stubs; nothing stale expected).

- [ ] **Step 5: Commit**

```bash
git add docfx sources/IcyUI.Tests/Docs/DocsMarkupSnippetTests.cs
git commit -m "Docs: binding kinds, x:DataType and Compiled=True"
```

---

### Task 5: Whole-branch check

- [ ] Release build: `dotnet build "sources/IcyUI.sln" -c Release` (0 errors; shipped libraries 0 warnings).
- [ ] Full test suite in Release; note the Total.
- [ ] Review the diff from the plan's first commit against the spec's Testing and Performance sections.
