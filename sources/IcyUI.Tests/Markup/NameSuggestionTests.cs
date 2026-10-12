using Icy.Markup;

namespace Icy.Tests.Markup
{
    public class NameSuggestionTests
    {
        private static readonly string[] Members = ["Title", "Volume", "Background"];

        [Theory]
        [InlineData("Titel", "Title")]
        [InlineData("Voulme", "Volume")]
        [InlineData("Bakground", "Background")]
        [InlineData("title", "Title")]
        public void Nearest_SuggestsOneEditAway_IncludingATransposition(string typo, string expected)
        {
            Assert.Equal(expected, NameSuggestion.Nearest(typo, Members));
        }

        [Theory]
        [InlineData("Zzzzz")]
        [InlineData("Tilte2x")]
        public void Nearest_ReturnsNull_WhenNothingIsClose(string typo)
        {
            Assert.Null(NameSuggestion.Nearest(typo, Members));
        }
    }
}
