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
        /// <exception cref="FormatException">
        /// The text isn't a valid <c>.po</c> file; the message starts with <c>name(line):</c>.
        /// </exception>
        public static PoCatalog Parse(TextReader reader, string? sourceName = null)
        {
            ArgumentNullException.ThrowIfNull(reader);
            var parser = new Parser(sourceName ?? "PO file");
            int lineNumber = 0;
            for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
            {
                lineNumber++;
                parser.Read(line.Trim().TrimStart((char)0xFEFF), lineNumber);
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
