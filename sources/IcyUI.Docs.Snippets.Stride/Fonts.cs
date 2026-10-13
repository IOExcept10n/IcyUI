// Examples for docfx/*/getting-started/*.md ("Fonts on other platforms") and concepts/assets-and-fonts.md. Each #region
// is one example on a page.
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

    public class OwnFontsOnlyStartup : StartupScript
    {
        public override void Start()
        {
            #region own-fonts-only
            Game.UseIcyUI(ui => ui
                .ConfigureFonts()
                .UseSystemFonts(false)                    // the engine defaults turned them on
                .ImportFont("Fonts/MyFont.ttf")
                .WithDefaultFontFamily("My Font"));        // Segoe UI is a system font
            #endregion
        }
    }

    public class BitmapFontStartup : StartupScript
    {
        public override void Start()
        {
            #region bitmap-font
            Game.UseIcyUI(ui => ui
                .ConfigureFonts()
                .ImportFont("Fonts/PixelFont.fnt"));       // its page images sit next to it
            #endregion
        }
    }
}
