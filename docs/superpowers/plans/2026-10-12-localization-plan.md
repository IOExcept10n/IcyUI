# Localization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** `{Localize Key}` in markup and `element.Localize(...)` in code show strings from a pluggable `ILocalizer`
(dictionary, RESX, gettext `.po`, or a chain of them), and setting `IcyConfiguration.Culture` switches every localized
property to the new language.

**Architecture:** A new `Icy.Localization` namespace holds `ILocalizer` (`string? Localize(string key, CultureInfo
culture)`) and the four localizers. `IcyConfiguration` gets `CultureChanged`, `Localize(key, context)` (the missing-key
rule) and an internal registry of localized properties (weak targets), re-applied once per culture change.
`LocalizeExtension` and `UIElementExtensions.Localize` resolve the string and register the property.

**Tech Stack:** C# / .NET 10, xUnit, `System.Resources.ResourceManager`, `Microsoft.Extensions.Logging` source-generated
messages.

**Spec:** `docs/superpowers/specs/2026-10-12-localization-design.md`

## Global Constraints

- Core project `sources/IcyUI` only, plus `Shared Samples`, the two sample hosts' `.csproj`, docs. No engine project
  code changes.
- Zero warnings in shipped libraries; StyleCop member order (fields, constructors, events, properties, methods; public
  before internal before private). Block-scoped namespaces; the two-line copyright header on new files in
  `sources/IcyUI`.
- Complete XML docs on every public member.
- New log message: `IcyLog` event **4001**, Warning, once per (key, context, culture). The class summary lists "4xxx
  localization".
- No per-frame work anywhere in this feature.
- `MonoGame Sample` has `Nullable` and `ImplicitUsings` off: shared sample files start with `#nullable enable` and
  explicit `using`s (see `Shared Samples/SampleCatalog.cs`).
- Run tests: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj"`; always check "Total".

## Spec deviations

- §2 says the `.po` reader is "a new `IAssetImporter<PoCatalog>` with `.po` mapped to `text/x-gettext-translation`".
  `PoLocalizer` reads the files through the configuration's `IAssetContext` (`IsAvailable`, `OpenStream`) instead, so
  nothing needs registering and no MIME entry is added; it's still the asset pipeline's file layer, per engine.
  `PoCatalog.Parse` stays public for anyone who wants to load a catalog themselves.
- §2 file names: `PoLocalizer` tries `ru-RU.po` and also gettext's own `ru_RU.po`.

## Review Focus

- A `{Localize}` on `Content` (an `object`/element property): must show a `TextBlock`, and switch language too. Pinned
  in Task 4.
- The game assigns its own text to a localized property, then the language changes: the game's text must stay. Pinned in
  Task 4.
- `Culture` set to the culture it already is (or `null` while `CurrentUICulture` is that culture): no re-apply, no event.
  Pinned in Task 3.
- An element localized from code before it's attached to a canvas: must get its text on attach and follow later
  switches. Pinned in Task 4.
- A `.po` file with Windows line endings, a UTF-8 BOM and a `#, fuzzy, c-format` flag line: must parse, skip the fuzzy
  entry. Pinned in Task 2.

---

### Task 1: `ILocalizer` and the code-side localizers

**Files:**
- Delete: `sources/IcyUI/Assets/ILocalizer.cs`, `sources/IcyUI/Assets/ResourceLocalizer.cs`
- Create: `sources/IcyUI/Localization/ILocalizer.cs`, `DictionaryLocalizer.cs`, `ResourceManagerLocalizer.cs`,
  `CompositeLocalizer.cs`
- Modify: `sources/IcyUI/Assets/IResourceRegistry.cs`, `sources/IcyUI/Assets/ResourceRegistry.cs` (drop
  `PreferredCulture` and the constructor parameter)
- Modify: `sources/IcyUI/Configuration/AssetConfiguration.cs` (default localizer), `BuildingExtensions.cs`
  (`WithLocalizer` docs, `using Icy.Localization;`)
- Test: `sources/IcyUI.Tests/Localization/LocalizerTests.cs` (create)

**Interfaces:**
- Produces: `Icy.Localization.ILocalizer { string? Localize(string key, CultureInfo culture); }`;
  `DictionaryLocalizer : ILocalizer, IEnumerable` with `Add(CultureInfo, string key, string value)` and
  `Add(string key, string value)` (invariant);
  `ResourceManagerLocalizer(ResourceManager resources)`; `CompositeLocalizer(params ILocalizer[] localizers)`.
  `AssetConfiguration.Localizer` defaults to `new DictionaryLocalizer()`.

- [ ] **Step 1: Write the failing tests**

```csharp
using System.Globalization;
using System.Resources;
using Icy.Localization;

namespace Icy.Tests.Localization
{
    public class LocalizerTests
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");
        private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru");
        private static readonly CultureInfo RussianRussia = CultureInfo.GetCultureInfo("ru-RU");

        [Fact]
        public void Dictionary_FallsBackThroughParentsToInvariant()
        {
            var localizer = new DictionaryLocalizer
            {
                { Russian, "Menu.Play", "Играть" },
                { "Menu.Quit", "Quit" },
            };

            Assert.Equal("Играть", localizer.Localize("Menu.Play", RussianRussia));
            Assert.Equal("Quit", localizer.Localize("Menu.Quit", RussianRussia));
            Assert.Null(localizer.Localize("Menu.Play", English));
            Assert.Null(localizer.Localize("Nothing", Russian));
        }

        [Fact]
        public void ResourceManager_AsksForTheCulture()
        {
            var localizer = new ResourceManagerLocalizer(new FakeResources());

            Assert.Equal("ru:Menu.Play", localizer.Localize("Menu.Play", Russian));
            Assert.Null(localizer.Localize("Missing", Russian));
        }

        [Fact]
        public void Composite_FirstNonNullWins()
        {
            var mods = new DictionaryLocalizer { { "Menu.Play", "PLAY!" } };
            var game = new DictionaryLocalizer { { "Menu.Play", "Play" }, { "Menu.Quit", "Quit" } };
            var localizer = new CompositeLocalizer(mods, game);

            Assert.Equal("PLAY!", localizer.Localize("Menu.Play", English));
            Assert.Equal("Quit", localizer.Localize("Menu.Quit", English));
            Assert.Null(localizer.Localize("Nothing", English));
        }

        private sealed class FakeResources : ResourceManager
        {
            public override string? GetString(string name, CultureInfo? culture) =>
                name == "Missing" ? null : $"{culture!.Name}:{name}";
        }
    }
}
```

- [ ] **Step 2: Run to verify they fail**

Run: `dotnet test "sources/IcyUI.Tests/IcyUI.Tests.csproj" --filter "FullyQualifiedName~LocalizerTests"`
Expected: compile errors (namespace `Icy.Localization` doesn't exist).

- [ ] **Step 3: Implement**

`sources/IcyUI/Localization/ILocalizer.cs`:

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Globalization;

namespace Icy.Localization
{
    /// <summary>
    /// A source of localized strings: localization files, RESX resources, a dictionary filled from code, or any
    /// other place a game keeps its translations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// IcyUI asks the configuration's localizer (<see cref="Configuration.AssetConfiguration.Localizer"/>) for every
    /// <c>{Localize}</c> and <see cref="UI.UIElementExtensions.Localize"/>, in the configuration's
    /// <see cref="Configuration.IcyConfiguration.Culture"/>. Built in: <see cref="DictionaryLocalizer"/>,
    /// <see cref="ResourceManagerLocalizer"/>, <see cref="PoLocalizer"/>, and <see cref="CompositeLocalizer"/> to
    /// chain them.
    /// </para>
    /// </remarks>
    public interface ILocalizer
    {
        /// <summary>
        /// Gets the string for <paramref name="key"/> in <paramref name="culture"/>.
        /// </summary>
        /// <param name="key">
        /// The key. A key with a context is <c>context + '\u0004' + key</c>, as gettext stores it.
        /// </param>
        /// <param name="culture">The language to localize into.</param>
        /// <returns>
        /// The string, or <see langword="null"/> when this localizer has none: the caller decides what to show.
        /// Implementations try <paramref name="culture"/>'s parents (<c>ru-RU</c>, then <c>ru</c>) when their source
        /// supports it.
        /// </returns>
        string? Localize(string key, CultureInfo culture);
    }
}
```

`DictionaryLocalizer.cs`:

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Collections;
using System.Globalization;

namespace Icy.Localization
{
    /// <summary>
    /// Localized strings added from code, per culture. Also a safety net at the end of a <see cref="CompositeLocalizer"/>.
    /// </summary>
    /// <remarks>
    /// A lookup tries the culture, its parents (<c>ru-RU</c>, then <c>ru</c>), then the strings added without a culture.
    /// Supports collection initializers: <c>new DictionaryLocalizer { { ru, "Menu.Play", "Играть" } }</c>.
    /// </remarks>
    public sealed class DictionaryLocalizer : ILocalizer, IEnumerable
    {
        private readonly Dictionary<(string Culture, string Key), string> strings = [];

        /// <summary>
        /// Adds or replaces the string for <paramref name="key"/> in <paramref name="culture"/>.
        /// </summary>
        /// <param name="culture">The language of <paramref name="value"/>.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The localized string.</param>
        public void Add(CultureInfo culture, string key, string value)
        {
            ArgumentNullException.ThrowIfNull(culture);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
            strings[(culture.Name, key)] = value;
        }

        /// <summary>
        /// Adds or replaces the string for <paramref name="key"/> used when no culture-specific one exists.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The string.</param>
        public void Add(string key, string value) => Add(CultureInfo.InvariantCulture, key, value);

        /// <inheritdoc/>
        public string? Localize(string key, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(culture);
            for (CultureInfo current = culture; ; current = current.Parent)
            {
                if (strings.TryGetValue((current.Name, key), out string? value))
                    return value;
                if (current.Equals(CultureInfo.InvariantCulture))
                    return null;
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => strings.GetEnumerator();
    }
}
```

`ResourceManagerLocalizer.cs`:

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Globalization;
using System.Resources;

namespace Icy.Localization
{
    /// <summary>
    /// Localized strings from .NET resources (<c>.resx</c> files compiled into the game and its satellite assemblies).
    /// </summary>
    /// <param name="resources">The resources, such as a generated <c>Strings.ResourceManager</c>.</param>
    /// <remarks>
    /// <see cref="ResourceManager"/> already falls back from <c>ru-RU</c> to <c>ru</c> to the neutral resources. The
    /// strings ship inside the build; use <see cref="PoLocalizer"/> for files players or modders can edit.
    /// </remarks>
    public sealed class ResourceManagerLocalizer(ResourceManager resources) : ILocalizer
    {
        private readonly ResourceManager resources = resources ?? throw new ArgumentNullException(nameof(resources));

        /// <inheritdoc/>
        public string? Localize(string key, CultureInfo culture) => resources.GetString(key, culture);
    }
}
```

`CompositeLocalizer.cs`:

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Globalization;

namespace Icy.Localization
{
    /// <summary>
    /// Chains localizers: asks each in order, and the first string found wins.
    /// </summary>
    /// <param name="localizers">The localizers, the most important first: mod files before the game's own, a
    /// <see cref="DictionaryLocalizer"/> safety net last.</param>
    public sealed class CompositeLocalizer(params ILocalizer[] localizers) : ILocalizer
    {
        private readonly ILocalizer[] localizers = localizers ?? throw new ArgumentNullException(nameof(localizers));

        /// <summary>
        /// Gets the localizers, in the order they're asked.
        /// </summary>
        public IReadOnlyList<ILocalizer> Localizers => localizers;

        /// <inheritdoc/>
        public string? Localize(string key, CultureInfo culture)
        {
            foreach (ILocalizer localizer in localizers)
            {
                if (localizer.Localize(key, culture) is { } text)
                    return text;
            }

            return null;
        }
    }
}
```

`PoLocalizer` is referenced from `ILocalizer`'s docs and arrives in Task 2; until then, write that `<see cref>` as
`<c>PoLocalizer</c>` and switch it to a `cref` in Task 2 (a broken `cref` is a build warning).

Then:
- Delete `Assets/ILocalizer.cs` and `Assets/ResourceLocalizer.cs`.
- `IResourceRegistry`: remove `PreferredCulture` (and its `using System.Globalization;` if now unused).
  `ResourceRegistry`: remove the primary-constructor parameter `preferredCulture` and the property; fix its XML docs.
- `AssetConfiguration`: `Localizer = new DictionaryLocalizer();`, property type `Icy.Localization.ILocalizer`, docs:
  "Gets or sets where localized strings come from. Defaults to an empty `DictionaryLocalizer`: every key shows as is
  until the game sets one (see `WithLocalizer`, `UsePoLocalization`)."
- `BuildingExtensions.WithLocalizer`: add `ArgumentNullException.ThrowIfNull` and a remark pointing at the built-in
  localizers.

`grep -rn "ILocalizer\|ResourceLocalizer\|PreferredCulture" sources --include=*.cs` must show only the new files.

- [ ] **Step 4: Run the tests**

Run the filter (3 passed), then the full suite and a build of `sources/IcyUI` (0 warnings).

- [ ] **Step 5: Commit**

```bash
git add -A sources/IcyUI sources/IcyUI.Tests/Localization
git commit -m "Localization: ILocalizer takes a key and a culture; dictionary, RESX and composite localizers"
```

---

### Task 2: gettext `.po` files

**Files:**
- Create: `sources/IcyUI/Localization/PoCatalog.cs`, `sources/IcyUI/Localization/PoLocalizer.cs`
- Modify: `sources/IcyUI/Configuration/BuildingExtensions.cs` (`UsePoLocalization`)
- Modify: `sources/IcyUI/Localization/ILocalizer.cs` (`<see cref="PoLocalizer"/>`)
- Test: `sources/IcyUI.Tests/Localization/PoCatalogTests.cs`, `PoLocalizerTests.cs` (create)

**Interfaces:**
- Produces: `public sealed class PoCatalog { int Count; bool TryGetString(string key, out string? value); static PoCatalog Parse(TextReader reader, string? sourceName = null); }`;
  `public sealed class PoLocalizer(AssetConfiguration assets, string folder, CultureInfo defaultCulture) : ILocalizer`;
  `IAssetConfigurationBuilder UsePoLocalization(this IAssetConfigurationBuilder builder, string folder, CultureInfo defaultCulture)`.

- [ ] **Step 1: Parser tests**

```csharp
using Icy.Localization;

namespace Icy.Tests.Localization
{
    public class PoCatalogTests
    {
        private static PoCatalog Parse(string text) => PoCatalog.Parse(new StringReader(text), "test.po");

        private static string? Get(PoCatalog catalog, string key) => catalog.TryGetString(key, out string? value) ? value : null;

        [Fact]
        public void Entries_Contexts_AndTheHeaderSkipped()
        {
            PoCatalog catalog = Parse(
                """
                msgid ""
                msgstr "Content-Type: text/plain; charset=UTF-8\n"

                # A translator comment
                msgid "Menu.Play"
                msgstr "Играть"

                msgctxt "Verb"
                msgid "Open"
                msgstr "Открыть"
                """);

            Assert.Equal(2, catalog.Count);
            Assert.Equal("Играть", Get(catalog, "Menu.Play"));
            Assert.Equal("Открыть", Get(catalog, "Verb\u0004Open"));
            Assert.Null(Get(catalog, "Open"));
        }

        [Fact]
        public void MultiLineStrings_AndEscapes()
        {
            PoCatalog catalog = Parse(
                """
                msgid "Help"
                msgstr ""
                "Line one\n"
                "Say \"hi\"\tand \\ go"
                """);

            Assert.Equal("Line one\nSay \"hi\"\tand \\ go", Get(catalog, "Help"));
        }

        [Fact]
        public void FuzzyAndUntranslatedEntries_AreSkipped_EvenWithoutBlankLines()
        {
            PoCatalog catalog = Parse(
                "msgid \"A\"\r\nmsgstr \"a\"\r\n#, fuzzy, c-format\r\nmsgid \"B\"\r\nmsgstr \"b\"\r\nmsgid \"C\"\r\nmsgstr \"\"\r\n");

            Assert.Equal("a", Get(catalog, "A"));
            Assert.Null(Get(catalog, "B"));
            Assert.Null(Get(catalog, "C"));
        }

        [Fact]
        public void PluralEntries_UseTheFirstForm()
        {
            PoCatalog catalog = Parse(
                """
                msgid "coin"
                msgid_plural "coins"
                msgstr[0] "монета"
                msgstr[1] "монеты"
                msgstr[2] "монет"
                """);

            Assert.Equal("монета", Get(catalog, "coin"));
        }

        [Fact]
        public void ByteOrderMark_IsIgnored()
        {
            PoCatalog catalog = Parse("\uFEFFmsgid \"A\"\nmsgstr \"a\"\n");

            Assert.Equal("a", Get(catalog, "A"));
        }

        [Theory]
        [InlineData("msgid A\nmsgstr \"a\"", 1)]
        [InlineData("msgid \"A\"\nmsgstr \"a\\\"", 2)]
        [InlineData("msgid \"A\"\nfoo \"a\"", 2)]
        [InlineData("\"orphan\"", 1)]
        public void MalformedInput_NamesTheFileAndLine(string text, int line)
        {
            var ex = Assert.Throws<FormatException>(() => Parse(text));

            Assert.StartsWith($"test.po({line}):", ex.Message);
        }
    }
}
```

- [ ] **Step 2: Run to verify they fail** (`--filter "FullyQualifiedName~PoCatalogTests"`: compile errors).

- [ ] **Step 3: `PoCatalog`**

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Icy.Localization
{
    /// <summary>
    /// The translations of one gettext <c>.po</c> file: one language.
    /// </summary>
    /// <remarks>
    /// <para>Read by <see cref="Parse"/>, which supports:</para>
    /// <list type="bullet">
    /// <item><description><c>msgctxt</c>, <c>msgid</c>, <c>msgstr</c>, and strings continued on following lines;</description></item>
    /// <item><description>the escapes <c>\n</c>, <c>\t</c>, <c>\r</c>, <c>\"</c> and <c>\\</c>;</description></item>
    /// <item><description><c>#</c> comments.</description></item>
    /// </list>
    /// <para>
    /// It skips the header entry (<c>msgid ""</c>), entries flagged <c>#, fuzzy</c> and untranslated ones (an empty
    /// <c>msgstr</c>), as gettext does. A plural entry contributes its <c>msgstr[0]</c>. An entry with a context is
    /// stored as <c>context + '\u0004' + msgid</c>.
    /// </para>
    /// </remarks>
    public sealed class PoCatalog
    {
        private readonly Dictionary<string, string> strings;

        private PoCatalog(Dictionary<string, string> strings)
        {
            this.strings = strings;
        }

        /// <summary>
        /// Gets the number of translated entries.
        /// </summary>
        public int Count => strings.Count;

        /// <summary>
        /// Reads a <c>.po</c> file.
        /// </summary>
        /// <param name="reader">The file's text.</param>
        /// <param name="sourceName">The file's name, for error messages.</param>
        /// <returns>The catalog.</returns>
        /// <exception cref="FormatException">The text isn't a valid <c>.po</c> file; the message starts with
        /// <c>name(line):</c>.</exception>
        public static PoCatalog Parse(TextReader reader, string? sourceName = null)
        {
            ArgumentNullException.ThrowIfNull(reader);
            var parser = new Parser(sourceName ?? "PO file");
            int lineNumber = 0;
            for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                lineNumber++;
                parser.Read(line.Trim().TrimStart('\uFEFF'), lineNumber);
            }

            parser.Flush();
            return new PoCatalog(parser.Strings);
        }

        /// <summary>
        /// Gets the translation of <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <c>msgid</c>, or <c>context + '\u0004' + msgid</c>.</param>
        /// <param name="value">The translation, when there is one.</param>
        /// <returns><see langword="true"/> when the file translates <paramref name="key"/>.</returns>
        public bool TryGetString(string key, [NotNullWhen(true)] out string? value) => strings.TryGetValue(key, out value);

        private sealed class Parser(string sourceName)
        {
            private StringBuilder? context;
            private StringBuilder? id;
            private StringBuilder? text;
            private StringBuilder? current;
            private bool hasText;
            private bool fuzzy;
            private bool skipping;

            public Dictionary<string, string> Strings { get; } = new(StringComparer.Ordinal);

            public void Read(string line, int lineNumber)
            {
                if (line.Length == 0)
                {
                    Flush();
                    return;
                }

                if (line[0] == '#')
                {
                    // A comment starts the next entry, blank line or not.
                    if (hasText)
                        Flush();
                    if (line.StartsWith("#,", StringComparison.Ordinal) && line.Contains("fuzzy", StringComparison.Ordinal))
                        fuzzy = true;
                    return;
                }

                if (line[0] == '"')
                {
                    if (current == null && !skipping)
                        throw Error(lineNumber, "a string that continues no keyword");
                    current?.Append(Unquote(line, lineNumber));
                    return;
                }

                int space = line.IndexOf(' ', StringComparison.Ordinal);
                if (space < 0)
                    throw Error(lineNumber, $"expected a keyword and a string, got '{line}'");

                string keyword = line[..space];
                string value = Unquote(line[(space + 1)..].TrimStart(), lineNumber);
                switch (keyword)
                {
                    case "msgctxt":
                        if (hasText)
                            Flush();
                        Track(context = new StringBuilder(value));
                        break;
                    case "msgid":
                        if (hasText)
                            Flush();
                        Track(id = new StringBuilder(value));
                        break;
                    case "msgstr":
                    case "msgstr[0]":
                        hasText = true;
                        Track(text = new StringBuilder(value));
                        break;
                    case "msgid_plural":
                        Skip();
                        break;
                    default:
                        if (!keyword.StartsWith("msgstr[", StringComparison.Ordinal))
                            throw Error(lineNumber, $"unknown keyword '{keyword}'");
                        hasText = true;
                        Skip();
                        break;
                }
            }

            public void Flush()
            {
                if (id is { Length: > 0 } && text is { Length: > 0 } && !fuzzy)
                    Strings[context == null ? id.ToString() : $"{context}\u0004{id}"] = text.ToString();

                context = id = text = current = null;
                hasText = fuzzy = skipping = false;
            }

            private void Track(StringBuilder builder)
            {
                current = builder;
                skipping = false;
            }

            private void Skip()
            {
                current = null;
                skipping = true;
            }

            private string Unquote(string quoted, int lineNumber)
            {
                if (quoted.Length < 2 || quoted[0] != '"' || quoted[^1] != '"')
                    throw Error(lineNumber, $"expected a quoted string, got '{quoted}'");

                var result = new StringBuilder(quoted.Length - 2);
                for (int i = 1; i < quoted.Length - 1; i++)
                {
                    char c = quoted[i];
                    if (c != '\\')
                    {
                        result.Append(c);
                        continue;
                    }

                    if (++i >= quoted.Length - 1)
                        throw Error(lineNumber, "a string ends inside an escape");
                    result.Append(quoted[i] switch
                    {
                        'n' => '\n',
                        't' => '\t',
                        'r' => '\r',
                        var other => other,
                    });
                }

                return result.ToString();
            }

            private FormatException Error(int lineNumber, string message) => new($"{sourceName}({lineNumber}): {message}.");
        }
    }
}
```

Run the parser tests: 9 passed (the theory counts 4).

- [ ] **Step 4: Localizer tests**

```csharp
using System.Globalization;
using Icy.Assets;
using Icy.Configuration;
using Icy.Localization;

namespace Icy.Tests.Localization
{
    public sealed class PoLocalizerTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "IcyUI.Tests", Guid.NewGuid().ToString("N"));

        public PoLocalizerTests() => Directory.CreateDirectory(Path.Combine(root, "Localization"));

        public void Dispose() => Directory.Delete(root, recursive: true);

        private void Write(string fileName, string body) =>
            File.WriteAllText(Path.Combine(root, "Localization", fileName), body);

        private PoLocalizer Create() =>
            new(new AssetConfiguration(new AssetContext(root + Path.DirectorySeparatorChar)), "Localization", CultureInfo.GetCultureInfo("en"));

        [Fact]
        public void FallsBackFromRegionToLanguageToDefault()
        {
            Write("en.po", "msgid \"A\"\nmsgstr \"a-en\"\nmsgid \"B\"\nmsgstr \"b-en\"\nmsgid \"C\"\nmsgstr \"c-en\"\n");
            Write("ru.po", "msgid \"A\"\nmsgstr \"a-ru\"\nmsgid \"B\"\nmsgstr \"b-ru\"\n");
            Write("ru-RU.po", "msgid \"A\"\nmsgstr \"a-ru-RU\"\n");
            PoLocalizer localizer = Create();
            var culture = CultureInfo.GetCultureInfo("ru-RU");

            Assert.Equal("a-ru-RU", localizer.Localize("A", culture));
            Assert.Equal("b-ru", localizer.Localize("B", culture));
            Assert.Equal("c-en", localizer.Localize("C", culture));
            Assert.Null(localizer.Localize("D", culture));
        }

        [Fact]
        public void GettextUnderscoreFileNames_AreFound()
        {
            Write("pt_BR.po", "msgid \"A\"\nmsgstr \"a-pt-BR\"\n");

            Assert.Equal("a-pt-BR", Create().Localize("A", CultureInfo.GetCultureInfo("pt-BR")));
        }

        [Fact]
        public void EachFileIsReadOnce()
        {
            Write("ru.po", "msgid \"A\"\nmsgstr \"first\"\n");
            PoLocalizer localizer = Create();
            Assert.Equal("first", localizer.Localize("A", CultureInfo.GetCultureInfo("ru")));

            Write("ru.po", "msgid \"A\"\nmsgstr \"second\"\n");

            Assert.Equal("first", localizer.Localize("A", CultureInfo.GetCultureInfo("ru")));
        }

        [Fact]
        public void NoFiles_IsNotAnError()
        {
            Assert.Null(Create().Localize("A", CultureInfo.GetCultureInfo("de")));
        }

        [Fact]
        public void UsePoLocalization_SetsTheLocalizer()
        {
            var builder = new IcyConfigurationBuilder();
            builder.ConfigureRendering(new Icy.Tests.Rendering.FakeRenderContext()).ConfigureInput(new Icy.Tests.Input.FakeInputSystem()).ConfigureTypes();
            builder.ConfigureAssets().UsePoLocalization("Localization", CultureInfo.GetCultureInfo("en"));

            Assert.IsType<PoLocalizer>(builder.Build().Assets.Localizer);
        }
    }
}
```

- [ ] **Step 5: `PoLocalizer` and `UsePoLocalization`**

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Globalization;
using Icy.Assets;
using Icy.Configuration;

namespace Icy.Localization
{
    /// <summary>
    /// Localized strings from gettext <c>.po</c> files, one per language, shipped as plain files next to the game.
    /// </summary>
    /// <param name="assets">The asset configuration whose <see cref="AssetConfiguration.DefaultAssetContext"/> the files
    /// are read from.</param>
    /// <param name="folder">The folder of the files, relative to the asset context, such as <c>Localization</c>.</param>
    /// <param name="defaultCulture">The language used when a culture and its parents have no string.</param>
    /// <remarks>
    /// <para>
    /// For <c>ru-RU</c> it reads <c>ru-RU.po</c> (or gettext's <c>ru_RU.po</c>), then <c>ru.po</c>, then the default
    /// culture's file. Each file is read once, the first time its language is needed; a missing file is remembered
    /// as missing. Translators edit <c>.po</c> files with tools such as Poedit, Weblate or Crowdin, and players can add
    /// a language without a new build. See <see cref="PoCatalog"/> for what's read.
    /// </para>
    /// <para>Not thread-safe: use it from the thread that runs the UI.</para>
    /// </remarks>
    public sealed class PoLocalizer(AssetConfiguration assets, string folder, CultureInfo defaultCulture) : ILocalizer
    {
        private readonly AssetConfiguration assets = assets ?? throw new ArgumentNullException(nameof(assets));
        private readonly string folder = folder ?? throw new ArgumentNullException(nameof(folder));
        private readonly CultureInfo defaultCulture = defaultCulture ?? throw new ArgumentNullException(nameof(defaultCulture));
        private readonly Dictionary<string, PoCatalog?> catalogs = new(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public string? Localize(string key, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(culture);
            return Find(key, culture) ?? Find(key, defaultCulture);
        }

        private string? Find(string key, CultureInfo culture)
        {
            for (CultureInfo current = culture; !current.Equals(CultureInfo.InvariantCulture); current = current.Parent)
            {
                if (Catalog(current.Name) is { } catalog && catalog.TryGetString(key, out string? text))
                    return text;
            }

            return null;
        }

        private PoCatalog? Catalog(string cultureName)
        {
            if (catalogs.TryGetValue(cultureName, out PoCatalog? catalog))
                return catalog;

            IAssetContext context = assets.DefaultAssetContext;
            foreach (string name in (string[])[cultureName, cultureName.Replace('-', '_')])
            {
                string path = folder.Length == 0 ? $"{name}.po" : $"{folder.TrimEnd('/', '\\')}/{name}.po";
                if (!context.IsAvailable(path))
                    continue;

                using Stream stream = context.OpenStream(path);
                using var reader = new StreamReader(stream);
                catalog = PoCatalog.Parse(reader, path);
                break;
            }

            catalogs[cultureName] = catalog;
            return catalog;
        }
    }
}
```

`StreamReader` detects a UTF-8 BOM itself. In `BuildingExtensions`, next to `WithLocalizer`:

```csharp
        /// <summary>
        /// Takes localized strings from gettext <c>.po</c> files, one per language, in <paramref name="folder"/>
        /// (see <see cref="PoLocalizer"/>).
        /// </summary>
        /// <param name="builder">The asset configuration builder instance.</param>
        /// <param name="folder">The folder of the files, relative to the game's assets, such as <c>Localization</c>.</param>
        /// <param name="defaultCulture">The language used when the current one has no string, usually the game's own.</param>
        /// <returns>The current asset configuration builder instance for fluent configuration.</returns>
        public static IAssetConfigurationBuilder UsePoLocalization(this IAssetConfigurationBuilder builder, string folder, CultureInfo defaultCulture)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.Assets.Localizer = new PoLocalizer(builder.Assets, folder, defaultCulture);
            return builder;
        }
```

- [ ] **Step 6: Run the tests** (`--filter "FullyQualifiedName~Po"`: 14 passed), full suite, 0 warnings.

- [ ] **Step 7: Commit**

```bash
git add -A sources/IcyUI sources/IcyUI.Tests/Localization
git commit -m "Localization: gettext .po files (PoCatalog, PoLocalizer, UsePoLocalization)"
```

---

### Task 3: `IcyConfiguration`: culture changes and the missing-key rule

**Files:**
- Modify: `sources/IcyUI/Configuration/IcyConfiguration.cs`
- Modify: `sources/IcyUI/Diagnostics/Logging/IcyLog.cs`
- Test: `sources/IcyUI.Tests/Localization/ConfigurationLocalizeTests.cs` (create)

**Interfaces:**
- Produces: `public event EventHandler? IcyConfiguration.CultureChanged`;
  `public string IcyConfiguration.Localize(string key, string? context = null)`;
  `IcyLog.LocalizedStringNotFound(ILogger logger, string key, string context, string culture)` (4001).

- [ ] **Step 1: Write the failing tests**

```csharp
using System.Globalization;
using Icy.Configuration;
using Icy.Localization;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Icy.Tests.Rendering;

namespace Icy.Tests.Localization
{
    public class ConfigurationLocalizeTests
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");
        private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru");

        private static (IcyConfiguration Configuration, CapturingLoggerFactory Log) Create()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureLogging(log)
                .Build();
            configuration.Assets.Localizer = new DictionaryLocalizer
            {
                { English, "Menu.Play", "Play" },
                { Russian, "Menu.Play", "Играть" },
                { Russian, "Verb\u0004Open", "Открыть" },
            };
            configuration.Culture = English;
            return (configuration, log);
        }

        [Fact]
        public void Localize_UsesTheConfigurationsCulture_AndContext()
        {
            var (configuration, _) = Create();
            Assert.Equal("Play", configuration.Localize("Menu.Play"));

            configuration.Culture = Russian;

            Assert.Equal("Играть", configuration.Localize("Menu.Play"));
            Assert.Equal("Открыть", configuration.Localize("Open", "Verb"));
        }

        [Fact]
        public void MissingKey_ShowsTheKey_AndWarnsOncePerCulture()
        {
            var (configuration, log) = Create();

            Assert.Equal("Menu.Quit", configuration.Localize("Menu.Quit"));
            configuration.Localize("Menu.Quit");
            configuration.Culture = Russian;
            configuration.Localize("Menu.Quit");

            Assert.Equal(2, log.Entries.Count(e => e.EventId.Id == 4001));
            Assert.Contains("Menu.Quit", log.Entries.First(e => e.EventId.Id == 4001).Message);
        }

        [Fact]
        public void CultureChanged_IsRaisedOnlyForADifferentCulture()
        {
            var (configuration, _) = Create();
            int raised = 0;
            configuration.CultureChanged += (_, _) => raised++;

            configuration.Culture = English;
            configuration.Culture = Russian;
            configuration.Culture = Russian;

            Assert.Equal(1, raised);
        }

        [Fact]
        public void ResettingToFollowTheUICulture_RaisesOnlyWhenTheCultureDiffers()
        {
            var (configuration, _) = Create();
            CultureInfo previous = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = English;
            try
            {
                int raised = 0;
                configuration.CultureChanged += (_, _) => raised++;

                configuration.Culture = null;

                Assert.Equal(0, raised);
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
            }
        }
    }
}
```

- [ ] **Step 2: Run to verify they fail** (compile errors: no `Localize`, no `CultureChanged`).

- [ ] **Step 3: Implement**

`IcyLog.cs`, after 3001:

```csharp
        [LoggerMessage(EventId = 4001, Level = LogLevel.Warning, Message = "No localized string for '{Key}' (context '{Context}') in '{Culture}'; the key is shown instead. Add it to the localizer's source for that language, or for the default one.")]
        public static partial void LocalizedStringNotFound(ILogger logger, string key, string context, string culture);
```

`IcyConfiguration.cs` (`using Icy.Diagnostics.Logging; using Icy.Localization;`): fields

```csharp
        private readonly HashSet<(string Key, string Culture)> reportedMissingStrings = [];
```

in the constructor, after `Fonts = new(this);`:

```csharp
            LocalizedProperties = new LocalizedPropertyRegistry();
            CultureChanged += (_, _) => LocalizedProperties.Reapply(this);
```

(`LocalizedPropertyRegistry` arrives in Task 4. For this task create it now as an empty internal class in
`sources/IcyUI/Localization/LocalizedPropertyRegistry.cs` with `internal void Reapply(IcyConfiguration configuration) { }`
and the copyright header; Task 4 fills it in.)

the event after the constructor:

```csharp
        /// <summary>
        /// Occurs when <see cref="Culture"/> changed to a different culture. IcyUI re-applies every localized property
        /// then (<c>{Localize}</c> and <see cref="UI.UIElementExtensions.Localize"/>).
        /// </summary>
        /// <remarks>
        /// Not raised while <see cref="Culture"/> follows <see cref="CultureInfo.CurrentUICulture"/> and that changes:
        /// set <see cref="Culture"/> to switch the game's language.
        /// </remarks>
        public event EventHandler? CultureChanged;
```

the `Culture` setter:

```csharp
            set
            {
                CultureInfo previous = Culture;
                culture = value;
                if (!Culture.Equals(previous))
                    CultureChanged?.Invoke(this, EventArgs.Empty);
            }
```

extend `Culture`'s remarks list with "localized strings (`{Localize}`, `Localize`)"; add after the public properties:

```csharp
        /// <summary>
        /// Gets the localized string for <paramref name="key"/> in <see cref="Culture"/>, from
        /// <see cref="AssetConfiguration.Localizer"/>.
        /// </summary>
        /// <param name="key">The key, such as <c>Menu.Play</c>.</param>
        /// <param name="context">An optional context that tells apart keys with the same text (gettext's <c>msgctxt</c>).</param>
        /// <returns>
        /// The string, or <paramref name="key"/> itself when the localizer has none: the text stays readable, and a
        /// warning (event 4001) is logged once per key and culture.
        /// </returns>
        public string Localize(string key, string? context = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            CultureInfo current = Culture;
            string lookup = context == null ? key : $"{context}\u0004{key}";
            if (Assets.Localizer.Localize(lookup, current) is { } text)
                return text;

            if (reportedMissingStrings.Add((lookup, current.Name)))
                IcyLog.LocalizedStringNotFound(Logging.CreateLogger("Icy.Localization"), key, context ?? string.Empty, current.Name);
            return key;
        }

        /// <summary>
        /// Gets the localized properties re-applied when <see cref="Culture"/> changes.
        /// </summary>
        internal LocalizedPropertyRegistry LocalizedProperties { get; }
```

- [ ] **Step 4: Run** (4 passed), full suite (the culture tests from item 2 still pass), 0 warnings.

- [ ] **Step 5: Commit**

```bash
git add -A sources/IcyUI sources/IcyUI.Tests/Localization
git commit -m "Localization: IcyConfiguration.Localize, CultureChanged, and the missing-key warning"
```

---

### Task 4: `{Localize}`, `Localize(...)` and switching

**Files:**
- Modify: `sources/IcyUI/Localization/LocalizedPropertyRegistry.cs`
- Create: `sources/IcyUI/Markup/Extensions/LocalizeExtension.cs`
- Modify: `sources/IcyUI/Markup/MarkupConfiguration.cs` (seed `"Localize"`), its remarks on the seeded names
- Modify: `sources/IcyUI/UI/UIElementExtensions.cs`
- Test: `sources/IcyUI.Tests/Localization/LocalizeTests.cs` (create)

**Interfaces:**
- Consumes: `IcyConfiguration.Localize`, `CultureChanged`, `LocalizedProperties` (Task 3); `MarkupMember.Resolve`,
  `GetValue`, `SetValue`, `PropertyType`, `Name`.
- Produces: `LocalizeExtension { string Key; string? Context; }` registered as `"Localize"`;
  `UIElementExtensions.Localize(this UIElement element, string propertyName, string key, string? context = null)`.

- [ ] **Step 1: Write the failing tests**

```csharp
using System.Globalization;
using Icy.Configuration;
using Icy.Localization;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Localization
{
    public class LocalizeTests
    {
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en");
        private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru");

        private static (IcyConfiguration Configuration, Canvas Canvas) Create()
        {
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .Build();
            configuration.Assets.Localizer = new DictionaryLocalizer
            {
                { English, "Menu.Play", "Play" },
                { Russian, "Menu.Play", "Играть" },
                { English, "Menu.Title", "Main menu" },
                { Russian, "Menu.Title", "Главное меню" },
                { Russian, "Verb\u0004Open", "Открыть" },
            };
            configuration.Culture = English;
            return (configuration, new Canvas(configuration));
        }

        private static T Load<T>(IcyConfiguration configuration, string body)
            where T : UIElement
        {
            var root = (StackPanel)new MarkupLoader(configuration).Load($"<StackPanel xmlns=\"https://icyui.dev/markup\">{body}</StackPanel>", "x.xml");
            return (T)root.Children[0];
        }

        [Fact]
        public void Markup_StringProperty_FollowsTheCulture()
        {
            var (configuration, _) = Create();
            var text = Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize Menu.Title}\"/>");
            Assert.Equal("Main menu", text.Text);

            configuration.Culture = Russian;

            Assert.Equal("Главное меню", text.Text);
        }

        [Fact]
        public void Markup_Content_BecomesATextBlock_AndFollowsTheCulture()
        {
            var (configuration, _) = Create();
            var button = Load<Button>(configuration, "<Button Content=\"{Localize Menu.Play}\"/>");
            Assert.Equal("Play", Assert.IsType<TextBlock>(button.Content).Text);

            configuration.Culture = Russian;

            Assert.Equal("Играть", Assert.IsType<TextBlock>(button.Content).Text);
        }

        [Fact]
        public void Markup_Context()
        {
            var (configuration, _) = Create();
            configuration.Culture = Russian;

            var text = Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize Open, Context=Verb}\"/>");

            Assert.Equal("Открыть", text.Text);
        }

        [Fact]
        public void Markup_NoKey_FailsAtLoad()
        {
            var (configuration, _) = Create();

            Assert.Throws<MarkupException>(() => Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize}\"/>"));
        }

        [Fact]
        public void TextTheGameSetItself_IsLeftAlone()
        {
            var (configuration, _) = Create();
            var text = Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize Menu.Title}\"/>");

            text.Text = "Score: 10";
            configuration.Culture = Russian;

            Assert.Equal("Score: 10", text.Text);
        }

        [Fact]
        public void Code_AttachedElement_FollowsTheCulture()
        {
            var (configuration, canvas) = Create();
            var text = new TextBlock();
            canvas.Add(text);

            text.Localize(nameof(TextBlock.Text), "Menu.Title");
            Assert.Equal("Main menu", text.Text);

            configuration.Culture = Russian;
            Assert.Equal("Главное меню", text.Text);
        }

        [Fact]
        public void Code_ElementLocalizedBeforeAttaching_GetsItsTextOnAttach()
        {
            var (configuration, canvas) = Create();
            var text = new TextBlock();

            text.Localize(nameof(TextBlock.Text), "Menu.Title");
            canvas.Add(text);
            Assert.Equal("Main menu", text.Text);

            configuration.Culture = Russian;
            Assert.Equal("Главное меню", text.Text);
        }

        [Fact]
        public void Code_UnknownProperty_Throws()
        {
            Assert.Throws<ArgumentException>(() => new TextBlock().Localize("Txet", "Menu.Title"));
        }

        [Fact]
        public void StyleSetter_ResolvesOnce()
        {
            var (configuration, canvas) = Create();
            var root = (StackPanel)new MarkupLoader(configuration).Load(
                "<StackPanel xmlns=\"https://icyui.dev/markup\"><StackPanel.Resources><Style TargetType=\"TextBlock\" Text=\"{Localize Menu.Title}\"/></StackPanel.Resources><TextBlock/></StackPanel>",
                "x.xml");
            canvas.Add(root);
            var text = (TextBlock)root.Children[0];
            Assert.Equal("Main menu", text.Text);

            configuration.Culture = Russian;

            Assert.Equal("Main menu", text.Text);
        }

        [Fact]
        public void CollectedElements_AreDropped()
        {
            var (configuration, _) = Create();
            LoadAndForget(configuration);
            GC.Collect();
            GC.WaitForPendingFinalizers();

            configuration.Culture = Russian;

            Assert.Equal(0, configuration.LocalizedProperties.Count);
        }

        private static void LoadAndForget(IcyConfiguration configuration) =>
            Load<TextBlock>(configuration, "<TextBlock Text=\"{Localize Menu.Title}\"/>");
    }
}
```

`CollectedElements_AreDropped` loads in a separate method so no local keeps the element alive; the root `StackPanel`
is never attached, so nothing else references the `TextBlock`. Run it in Release too: Debug can extend lifetimes.

- [ ] **Step 2: Run to verify they fail** (compile errors: no `Localize` extension method, unknown `{Localize}`).

- [ ] **Step 3: The registry**

Replace the Task 3 placeholder class with:

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;
using Icy.Markup;
using Icy.UI;

namespace Icy.Localization
{
    /// <summary>
    /// The localized properties of one configuration, re-applied when its culture changes. Holds its elements weakly
    /// and does nothing between changes.
    /// </summary>
    internal sealed class LocalizedPropertyRegistry
    {
        private readonly List<Entry> entries = [];

        /// <summary>
        /// Gets the number of properties tracked, for tests.
        /// </summary>
        public int Count => entries.Count;

        /// <summary>
        /// Converts a localized string to what <paramref name="member"/> holds: the string itself, or what the
        /// configuration's text converter makes of it (a <see cref="UI.Controls.TextBlock"/> for <c>Content</c>).
        /// </summary>
        public static object? ToPropertyValue(IcyConfiguration configuration, MarkupMember member, string text) =>
            member.PropertyType.IsAssignableFrom(typeof(string))
                ? text
                : configuration.Types.TypeConverter.Convert(text, member.PropertyType);

        /// <summary>
        /// Tracks <paramref name="member"/> of <paramref name="target"/>, which now holds <paramref name="value"/>.
        /// </summary>
        public void Register(UIElement target, MarkupMember member, string key, string? context, object? value) =>
            entries.Add(new Entry(new WeakReference<UIElement>(target), member, key, context, value));

        /// <summary>
        /// Gives every tracked property its string in the configuration's current culture. Drops collected elements,
        /// and properties the game has set to something else since.
        /// </summary>
        public void Reapply(IcyConfiguration configuration) => entries.RemoveAll(entry => !entry.Reapply(configuration));

        private sealed class Entry(WeakReference<UIElement> target, MarkupMember member, string key, string? context, object? value)
        {
            private object? lastValue = value;

            public bool Reapply(IcyConfiguration configuration)
            {
                if (!target.TryGetTarget(out UIElement? element) || !Equals(member.GetValue(element), lastValue))
                    return false;

                object? next = ToPropertyValue(configuration, member, configuration.Localize(key, context));
                member.SetValue(element, next);
                lastValue = next;
                return true;
            }
        }
    }
}
```

`ITypeConverter.Convert(object? value, Type targetType)` is the configuration's text converter, the one
`Binding.ConvertTextToTargetType` uses for the same string-to-`Content` conversion.

- [ ] **Step 4: `LocalizeExtension`**

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Localization;
using Icy.UI;

namespace Icy.Markup.Extensions
{
    /// <summary>
    /// <c>{Localize Key}</c>: the localized string for a key, in the configuration's
    /// <see cref="Configuration.IcyConfiguration.Culture"/>, updated when the culture changes.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><description>
    /// <c>{Localize Menu.Play}</c> is short for <c>{Localize Key=Menu.Play}</c>; <c>Context=</c> tells apart keys with the
    /// same text (gettext's <c>msgctxt</c>).
    /// </description></item>
    /// <item><description>
    /// On a property that doesn't take text, such as <c>Content</c>, the string goes through the configuration's text
    /// converter, as a plain string attribute does: <c>Content</c> gets a <see cref="UI.Controls.TextBlock"/>.
    /// </description></item>
    /// <item><description>
    /// A missing key shows the key itself (see <see cref="Configuration.IcyConfiguration.Localize"/>).
    /// </description></item>
    /// <item><description>
    /// In a style setter, the string is resolved once, when the style loads, and doesn't change with the culture:
    /// styles are shared by many elements. Templates are fine.
    /// </description></item>
    /// </list>
    /// </remarks>
    [MarkupExtensionDefaultProperty(nameof(Key))]
    public sealed class LocalizeExtension : IMarkupExtension
    {
        /// <summary>
        /// Gets or sets the key, such as <c>Menu.Play</c>.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets an optional context for the key, or <see langword="null"/>.
        /// </summary>
        public string? Context { get; set; }

        /// <inheritdoc/>
        /// <exception cref="MarkupException">No key is given, or the string can't be converted to the property's type.</exception>
        public object? ProvideValue(MarkupExtensionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (string.IsNullOrEmpty(Key))
                throw MarkupException.At("{Localize} needs a key, such as {Localize Menu.Play}.", context.Node, context.SourcePath);

            object? value;
            try
            {
                value = LocalizedPropertyRegistry.ToPropertyValue(context.Configuration, context.Member, context.Configuration.Localize(Key, Context));
            }
            catch (Exception ex) when (ex is not MarkupException)
            {
                throw MarkupException.At($"{{Localize {Key}}} can't set '{context.Member.Name}': {ex.Message}", context.Node, context.SourcePath, ex);
            }

            if (context.Target is UIElement element)
                context.Configuration.LocalizedProperties.Register(element, context.Member, Key, Context, value);
            return value;
        }
    }
}
```

Seed it in `MarkupConfiguration`: `["Localize"] = typeof(LocalizeExtension),`, and add it to the remarks that list the
seeded names.

- [ ] **Step 5: `UIElementExtensions.Localize`**

Add to `UIElementExtensions` (`using Icy.Configuration; using Icy.Data.Markup; using Icy.Localization; using Icy.Markup;`):

```csharp
        /// <summary>
        /// Sets <paramref name="propertyName"/> to the localized string for <paramref name="key"/>, and keeps it updated
        /// when the configuration's <see cref="IcyConfiguration.Culture"/> changes: <c>{Localize}</c> for UI built in code.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="propertyName">The property, such as <c>nameof(TextBlock.Text)</c>.</param>
        /// <param name="key">The key, such as <c>Menu.Play</c>.</param>
        /// <param name="context">An optional context for the key.</param>
        /// <remarks>
        /// An element that isn't on a canvas yet gets its string when it's attached: the configuration comes from the
        /// canvas. Setting the property to something else later stops the updates.
        /// </remarks>
        /// <exception cref="ArgumentException"><paramref name="propertyName"/> isn't a property of the element.</exception>
        public static void Localize(this UIElement element, string propertyName, string key, string? context = null)
        {
            ArgumentNullException.ThrowIfNull(element);
            ArgumentNullException.ThrowIfNull(key);
            MarkupMember member = MarkupMember.Resolve(element.GetType(), propertyName, PropertyRegistry.Current)
                ?? throw new ArgumentException($"'{propertyName}' isn't a property of '{element.GetType().Name}'.", nameof(propertyName));

            if (element.Canvas is { } canvas)
            {
                Apply(canvas.Configuration);
                return;
            }

            void OnAttached(object? sender, EventArgs e)
            {
                element.Attached -= OnAttached;
                Apply(element.Canvas!.Configuration);
            }

            element.Attached += OnAttached;

            void Apply(IcyConfiguration configuration)
            {
                object? value = LocalizedPropertyRegistry.ToPropertyValue(configuration, member, configuration.Localize(key, context));
                member.SetValue(element, value);
                configuration.LocalizedProperties.Register(element, member, key, context, value);
            }
        }
```

`Canvas.Configuration` is `internal`, visible here (same assembly).

- [ ] **Step 6: Run** (`--filter "FullyQualifiedName~LocalizeTests"`: 10 passed), full suite, 0 warnings.

- [ ] **Step 7: Commit**

```bash
git add -A sources/IcyUI sources/IcyUI.Tests/Localization
git commit -m "Localization: {Localize} and Localize(...), re-applied when the culture changes"
```

---

### Task 5: `LocalizationDemo` in both sample hosts

**Files:**
- Create: `sources/Shared Samples/LocalizationDemo.cs`, `sources/Shared Samples/Localization/en.po`,
  `sources/Shared Samples/Localization/ru.po`
- Modify: `sources/Shared Samples/SampleCatalog.cs`, `sources/Stride Sample/Stride Sample.csproj`,
  `sources/MonoGame Sample/MonoGame Sample.csproj`

- [ ] **Step 1: The `.po` files**

`en.po`:

```
msgid "Demo.Title"
msgstr "Main menu"

msgid "Demo.Play"
msgstr "Play"

msgid "Demo.Settings"
msgstr "Settings"

msgid "Demo.Volume"
msgstr "Volume"

msgid "Demo.Hint"
msgstr "Switch the language: the texts and the number format change together."
```

`ru.po` (save as UTF-8):

```
msgid "Demo.Title"
msgstr "Главное меню"

msgid "Demo.Play"
msgstr "Играть"

msgid "Demo.Settings"
msgstr "Настройки"

msgid "Demo.Volume"
msgstr "Громкость"

msgid "Demo.Hint"
msgstr "Переключите язык: тексты и формат чисел меняются вместе."
```

- [ ] **Step 2: The demo**

```csharp
// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information

// Linked into both sample hosts; MonoGame Sample has nullable disabled project-wide.
#nullable enable
using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Configuration;
using Icy.Localization;
using Icy.Markup;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.SharedSamples
{
    /// <summary>
    /// <c>{Localize}</c> with gettext <c>.po</c> files, and one language switch for both texts and numbers.
    /// </summary>
    public static class LocalizationDemo
    {
        /// <summary>
        /// The page. Its texts use Segoe UI (through an implicit style) for the Cyrillic letters the sample font lacks.
        /// </summary>
        public const string Markup =
            """
            <StackPanel Orientation="Vertical" Margin="20">
              <StackPanel.Resources>
                <Style TargetType="TextBlock" FontFamily="Segoe UI"/>
              </StackPanel.Resources>
              <TextBlock Text="{Localize Demo.Title}" FontSize="24" Margin="0,0,0,8"/>
              <TextBlock Text="{Localize Demo.Hint}" Margin="0,0,0,12"/>
              <Button Content="{Localize Demo.Play}" HorizontalAlignment="Left" Margin="0,0,0,6"/>
              <Button Content="{Localize Demo.Settings}" HorizontalAlignment="Left" Margin="0,0,0,12"/>
              <StackPanel Orientation="Horizontal" Margin="0,0,0,12">
                <TextBlock Text="{Localize Demo.Volume}" Width="120" VerticalAlignment="Center"/>
                <TextBox Text="{Binding Volume, Mode=TwoWay}" Width="120"/>
              </StackPanel>
              <StackPanel Orientation="Horizontal">
                <Button x:Name="english" Margin="0,0,8,0">English</Button>
                <Button x:Name="russian">Русский</Button>
              </StackPanel>
            </StackPanel>
            """;

        /// <summary>
        /// Builds the page, and makes <c>Localization/*.po</c> the configuration's localizer.
        /// </summary>
        /// <param name="configuration">The library configuration.</param>
        /// <param name="fontFamily">Unused: the page sets its own font for Cyrillic.</param>
        /// <returns>The root element.</returns>
        public static UIElement Build(IcyConfiguration configuration, string fontFamily)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            configuration.Assets.Localizer = new PoLocalizer(configuration.Assets, "Localization", CultureInfo.GetCultureInfo("en"));
            configuration.Culture = CultureInfo.GetCultureInfo("en");

            UIElement root = new MarkupLoader(configuration).Load(Markup, nameof(LocalizationDemo));
            var viewModel = new VolumeViewModel();
            root.DataContext = viewModel;

            root.FindRequiredControl<Button>("english").Click += (_, _) => Switch("en");
            root.FindRequiredControl<Button>("russian").Click += (_, _) => Switch("ru");
            return root;

            void Switch(string culture)
            {
                configuration.Culture = CultureInfo.GetCultureInfo(culture);

                // Texts follow at once; a binding shows the new number format on its next update.
                viewModel.Refresh();
            }
        }

        private sealed class VolumeViewModel : ObservableObject
        {
            private float volume = 0.75f;

            public float Volume
            {
                get => volume;
                set => SetProperty(ref volume, value);
            }

            public void Refresh() => OnPropertyChanged(nameof(Volume));
        }
    }
}
```

`FindRequiredControl<T>` is an extension in `Icy.UI.UIElementExtensions` (namespace `Icy.UI`, already imported).

- [ ] **Step 3: Register it**

`SampleCatalog.cs`: after the `"Control Templates"` entry, `new("Markup", "Localization", LocalizationDemo.Build),`.

Both host `.csproj` files: next to the other `Compile Include` links,

```xml
    <Compile Include="..\Shared Samples\LocalizationDemo.cs" Link="LocalizationDemo.cs" />
```

and in an `ItemGroup` of `None` items,

```xml
    <None Include="..\Shared Samples\Localization\*.po" LinkBase="Localization" CopyToOutputDirectory="PreserveNewest" />
```

- [ ] **Step 4: Build both hosts**

Run: `dotnet build "sources/Stride Sample/Stride Sample.csproj"` and
`dotnet build "sources/MonoGame Sample/MonoGame Sample.csproj"`. Both succeed;
`ls "sources/Stride Sample/bin"/*/*/Localization` (or the host's output folder) shows `en.po` and `ru.po`.
Don't run the samples: Ivan smoke-tests them.

- [ ] **Step 5: Commit**

```bash
git add "sources/Shared Samples" "sources/Stride Sample/Stride Sample.csproj" "sources/MonoGame Sample/MonoGame Sample.csproj"
git commit -m "Samples: localization demo with .po files and a language switch"
```

---

### Task 6: Docs

**Files:**
- Create: `docfx/en/concepts/localization.md`, `docfx/ru/concepts/localization.md` (stub)
- Modify: `docfx/en/concepts/toc.yml`, `docfx/ru/concepts/toc.yml`, `docfx/en/concepts/index.md`
- Create: `docfx/snippets/markup/concepts/localize.xml`; modify `sources/IcyUI.Tests/Docs/DocsMarkupSnippetTests.cs`
- Create: `sources/IcyUI.Docs.Snippets.Stride/Localization.cs`
- Modify: `docfx/en/concepts/configuration.md` (the `WithCulture` line), `data-binding.md` (the culture paragraph),
  `logging-and-diagnostics.md` (event 4001), `docfx/en/markup/index.md` (`{Localize}`)

- [ ] **Step 1: Snippets**

`docfx/snippets/markup/concepts/localize.xml`:

```xml
<!-- {Localize} shows the string for a key in the configuration's culture, and follows it when it changes. -->
<StackPanel Orientation="Vertical">
  <TextBlock Text="{Localize Menu.Title}" />
  <Button Content="{Localize Menu.Play}" />
</StackPanel>
```

Test in `DocsMarkupSnippetTests`. Its `Load(string file)` helper builds its own configuration; split it so the new test
can pass one:

```csharp
        private static UIElement Load(string file)
        {
            var configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
            return Load(file, configuration);
        }

        private static UIElement Load(string file, IcyConfiguration configuration)
        {
            configuration.UseDefaultTheme();
            return new MarkupLoader(configuration).Load(File.ReadAllText(Path.Combine(SnippetRoot, file)), file);
        }
```

Then:

```csharp
        [Fact]
        public void Localize_FollowsTheCulture()
        {
            var configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration());
            var russian = System.Globalization.CultureInfo.GetCultureInfo("ru");
            configuration.Assets.Localizer = new Icy.Localization.DictionaryLocalizer
            {
                { "Menu.Title", "Main menu" },
                { "Menu.Play", "Play" },
                { russian, "Menu.Title", "Главное меню" },
                { russian, "Menu.Play", "Играть" },
            };
            var root = (StackPanel)Load(Path.Combine("concepts", "localize.xml"), configuration);

            configuration.Culture = russian;

            Assert.Equal("Главное меню", Assert.IsType<TextBlock>(root.Children[0]).Text);
            Assert.Equal("Играть", Assert.IsType<TextBlock>(Assert.IsType<Button>(root.Children[1]).Content).Text);
        }
```

`sources/IcyUI.Docs.Snippets.Stride/Localization.cs`:

```csharp
// Examples for docfx/*/concepts/localization.md. Each #region is one example on a page.
namespace MyGame.Localization
{
    using System.Globalization;
    using Icy.Configuration;
    using Icy.Localization;
    using Icy.Stride.Configuration;
    using Icy.UI.Controls;
    using Stride.Engine;

    public class LocalizationStartup : StartupScript
    {
        public override void Start()
        {
            #region setup
            Game.UseIcyUI(ui => ui
                .ConfigureAssets()
                .UsePoLocalization("Localization", CultureInfo.GetCultureInfo("en")));  // Localization/en.po, ru.po, ...
            #endregion
        }
    }

    public static class LocalizationExamples
    {
        public static void Switch(IcyConfiguration configuration)
        {
            #region switch
            configuration.Culture = CultureInfo.GetCultureInfo("ru");   // every {Localize} changes now
            #endregion
        }

        public static void Chain(IcyConfiguration configuration)
        {
            #region chain
            var mods = new PoLocalizer(configuration.Assets, "Mods/Localization", CultureInfo.GetCultureInfo("en"));
            var game = new PoLocalizer(configuration.Assets, "Localization", CultureInfo.GetCultureInfo("en"));
            configuration.Assets.Localizer = new CompositeLocalizer(mods, game);
            #endregion
        }

        public static void FromCode(TextBlock score)
        {
            #region from-code
            score.Localize(nameof(TextBlock.Text), "Hud.Score");
            #endregion
        }
    }
}
```

Build it: `dotnet build sources/IcyUI.Docs.Snippets.Stride` (0 errors).

- [ ] **Step 2: The page**

`docfx/en/concepts/localization.md`:

```markdown
# Localization

IcyUI shows text in the player's language: markup asks for a string by key with `{Localize}`, a localizer finds it in
the current language, and changing the language updates every localized text at once.

## Setting it up

Choose where your translations live and give the configuration a localizer. With gettext `.po` files, one per
language in a `Localization` folder next to the game:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#setup)]

| Localizer | Strings come from | Good for |
|---|---|---|
| <xref:Icy.Localization.PoLocalizer> | gettext `.po` files, one per language (`en.po`, `ru.po`, `pt_BR.po`). | Most games: translators use tools like Poedit, Weblate or Crowdin, and players can add a language without a new build. |
| <xref:Icy.Localization.ResourceManagerLocalizer> | `.resx` resources compiled into the game. | Projects that already use .NET resources. |
| <xref:Icy.Localization.DictionaryLocalizer> | Strings added from code. | Prototypes, tests, a few built-in strings. |
| <xref:Icy.Localization.CompositeLocalizer> | Several localizers, asked in order. | Mod translations over the game's own, or a safety net. |

Any other source works too: implement <xref:Icy.Localization.ILocalizer>, one method that returns the string for a key
and a culture, or `null`.

A culture falls back to its parent and then to the default language: for `ru-RU`, `ru-RU.po`, then `ru.po`, then
`en.po`. A key that isn't found anywhere shows as the key itself, and IcyUI logs a warning once (event 4001).

## In markup

[!code-xml[](../../snippets/markup/concepts/localize.xml)]

- `{Localize Menu.Play}` works on any property that takes text. On `Content`, the string becomes a `TextBlock`, as plain
  text does.
- `Context=` tells apart two keys with the same text (gettext's `msgctxt`): `{Localize Open, Context=Verb}`.

## Switching languages

The language is the configuration's `Culture`, the same setting numbers are shown in:

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#switch)]

Every `{Localize}` text changes right away. A binding shows numbers in the new format the next time it updates. By
default, `Culture` follows the system's UI language; set it when your game has its own language setting.

## UI built in code

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#from-code)]

It updates with the language like `{Localize}`. `Configuration.Localize("Hud.Score")` returns a string for code that
formats text itself.

## Mods and layering

[!code-csharp[](../../../sources/IcyUI.Docs.Snippets.Stride/Localization.cs#chain)]

## Performance

A localized text costs one lookup when it loads and one small entry so it can change language. Each `.po` file is read
once, the first time its language is needed. Switching languages updates each localized text once; nothing runs between
switches.

## Pitfalls

- **Your own text replaces the localized one for good.** Setting a localized property yourself (a score, a player
  name) stops it from changing with the language.
- **Styles don't switch.** `{Localize}` in a style setter is read once, when the style loads. Put it on the elements, or
  in a template.
- **Missing glyphs.** A font without Cyrillic or CJK letters can't show those languages; pick a font that covers your
  languages.
- **Arguments.** `"Score: {0}"` isn't filled in for you yet: format it in code with `Configuration.Localize`.

## See also

- [Data binding](data-binding.md#conversion) for numbers in the current culture.
- [Configuration](configuration.md).
```

Russian stub `docfx/ru/concepts/localization.md`:

```markdown
---
translation: stub
---

# Локализация

Эта страница ещё не переведена. Читайте [английскую версию](https://icyui.dev/concepts/localization.html).
```

Add to both `toc.yml` files after "DPI and UI scaling" (en `- name: Localization`, ru `- name: Локализация`,
`href: localization.md`), and to `docfx/en/concepts/index.md`:
`- **[Localization](localization.md):** Showing your UI in the player's language: localizers, {Localize}, and switching languages.`

- [ ] **Step 3: The other pages**

- `configuration.md`: change the `WithCulture` sentence to "...the culture strings are localized in and numbers are
  shown and parsed in (see [Localization](localization.md) and [Data binding](data-binding.md#conversion))."
- `data-binding.md` culture paragraph: add "It's also the language of `{Localize}` texts (see
  [Localization](localization.md))."
- `logging-and-diagnostics.md` event table: `| 4001 | Warning | A localized string is missing for a key and language; the key is shown. |`
- `docfx/en/markup/index.md`: a `{Localize}` entry next to `{Binding}`: syntax, `Key`, `Context`, link to the page.

- [ ] **Step 4: Build docs and run tests**

`bash .github/scripts/build-docs.sh` (0 warnings), `bash .github/scripts/check-translations.sh`, full test suite.

- [ ] **Step 5: Commit**

```bash
git add docfx sources/IcyUI.Tests/Docs sources/IcyUI.Docs.Snippets.Stride/Localization.cs
git commit -m "Docs: localization concept page"
```

---

### Task 7: Whole-branch check

- [ ] `dotnet build "sources/IcyUI.sln" -c Release` (0 errors; shipped libraries 0 warnings).
- [ ] Full test suite in Release; note the Total.
- [ ] `grep -rn "ILocalizer\|PreferredCulture" docfx sources --include=*.md --include=*.cs` shows no stale references.
- [ ] Review the branch diff against the spec's Testing and Performance sections.
