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
