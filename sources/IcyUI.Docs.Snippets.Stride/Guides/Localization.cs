// Engine-neutral code for docfx/*/guides/localization.md. Each #region is one example on the page. This file is also
// compiled into IcyUI.Tests (and into the MonoGame snippets), so the guide's behaviour is tested. It must not reference an engine.
namespace MyGame.Localization
{
    using System.Collections.Generic;
    using System.Globalization;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Icy.Configuration;

    #region language-setting
    /// <summary>The Settings page's language choice. Choosing a language switches every localized text at once.</summary>
    public sealed partial class LanguageSetting : ObservableObject
    {
        private readonly IcyConfiguration configuration;
        private string selected;

        public LanguageSetting(IcyConfiguration configuration)
        {
            this.configuration = configuration;
            selected = NameOf(configuration.Culture);
        }

        /// <summary>The languages the game ships, as the player sees them (not translated).</summary>
        public IReadOnlyList<string> Languages { get; } = ["English", "Русский"];

        public string Selected
        {
            get => selected;
            set
            {
                if (SetProperty(ref selected, value))
                    configuration.Culture = CultureInfo.GetCultureInfo(value == "Русский" ? "ru" : "en");
            }
        }

        private static string NameOf(CultureInfo culture) => culture.TwoLetterISOLanguageName == "ru" ? "Русский" : "English";
    }
    #endregion
}
