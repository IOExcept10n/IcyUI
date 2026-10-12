// Examples for docfx/*/getting-started/*.md ("Fonts on other platforms"). Each #region is one example on a page.
namespace MyGame.OwnFontExample
{
    using Icy.Configuration;
    using Icy.Stride.Configuration;
    using Stride.Engine;

    public class FontStartup : StartupScript
    {
        public override void Start()
        {
            #region own-font
            Game.UseIcyUI(ui => ui
                .ConfigureFonts()
                .ImportFont("Fonts/MyFont.ttf")           // copied to the output folder
                .WithDefaultFontFamily("My Font"));        // the family name stored in the file
            #endregion
        }
    }
}
