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
            PoCatalog catalog = Parse((char)0xFEFF + "msgid \"A\"\nmsgstr \"a\"\n");

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
