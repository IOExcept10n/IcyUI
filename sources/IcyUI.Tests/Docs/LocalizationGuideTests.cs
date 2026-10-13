// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Globalization;
using Icy.Assets;
using Icy.Configuration;
using Icy.Localization;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using MyGame.Localization;

namespace Icy.Tests.Docs
{
    /// <summary>Behaviour of the "Localizing your game UI" guide: choosing a language in the setting switches the menu's texts.</summary>
    public sealed class LocalizationGuideTests
    {
        private readonly IcyConfiguration configuration;
        private readonly StackPanel menu;
        private readonly LanguageSetting setting;

        public LocalizationGuideTests()
        {
            configuration = new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()).UseDefaultTheme();
            var russian = CultureInfo.GetCultureInfo("ru");
            configuration.Culture = CultureInfo.GetCultureInfo("en");
            configuration.Assets.Localizer = new DictionaryLocalizer
            {
                { "Menu.Title", "My Game" },
                { "Menu.Play", "Play" },
                { "Settings.Language", "Language" },
                { russian, "Menu.Title", "Моя игра" },
                { russian, "Menu.Play", "Играть" },
                { russian, "Settings.Language", "Язык" },
            };
            string markup = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "DocsSnippets", "guides", "localization", "localized-menu.xml"));
            menu = (StackPanel)new MarkupLoader(configuration).Load(markup, "localized-menu.xml");
            setting = new LanguageSetting(configuration);
            menu.DataContext = setting;
        }

        [Fact]
        public void TheMenu_StartsInTheConfiguredLanguage()
        {
            Assert.Equal("My Game", menu.FindRequiredControl<TextBlock>("Title").Text);
            Assert.Equal("English", setting.Selected);
        }

        [Fact]
        public void ChoosingALanguage_SwitchesTheCultureAndEveryText()
        {
            setting.Selected = "Русский";

            Assert.Equal("ru", configuration.Culture.TwoLetterISOLanguageName);
            Assert.Equal("Моя игра", menu.FindRequiredControl<TextBlock>("Title").Text);
            Assert.Equal("Играть", Assert.IsType<TextBlock>(menu.FindRequiredControl<Button>("Play").Content).Text);
        }

        [Fact]
        public void TheComboBox_ChangesTheLanguage()
        {
            var combo = menu.FindRequiredControl<ComboBox>("Language");

            combo.SelectedItem = "Русский";

            Assert.Equal("Русский", setting.Selected);
            Assert.Equal("Моя игра", menu.FindRequiredControl<TextBlock>("Title").Text);
        }
    }
}
