// Examples for docfx/*/concepts/localization.md. Each #region is one example on a page.
namespace MyGame.Localization
{
    using System.Globalization;
    using Icy.Configuration;
    using Icy.Localization;
    using Icy.Stride.Configuration;
    using Icy.UI;
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
            var mods = new PoLocalizer(configuration.Assets, "Mods/Localization");   // no default: only the player's language
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
