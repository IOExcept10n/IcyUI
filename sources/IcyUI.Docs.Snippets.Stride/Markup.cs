// Examples for docfx/*/markup/index.md. Each #region is one example on a page.
namespace MyGame.ShortNamesExample
{
    using Icy.Configuration;
    using Icy.Stride.Configuration;
    using Icy.UI.Controls;
    using Stride.Engine;

    /// <summary>A custom control the examples register under a short name.</summary>
    public class HealthBar : ProgressBar
    {
    }

    public static class MarkupSetup
    {
        public static void Register(Game game)
        {
            #region short-names
            IcyConfiguration configuration = game.GetIcyConfiguration();
            configuration.Types.Markup.RegisterShortName<HealthBar>();          // <HealthBar/>
            configuration.Types.Markup.RegisterShortName<HealthBar>("Health");  // <Health/>
            #endregion
        }
    }
}
