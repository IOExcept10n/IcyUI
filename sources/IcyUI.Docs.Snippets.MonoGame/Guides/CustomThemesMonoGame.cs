// MonoGame code for docfx/*/guides/custom-themes.md.
namespace MyGame.Themes
{
    using Icy.Configuration;
    using Icy.MonoGame.Configuration;
    using Microsoft.Xna.Framework;

    public static class ThemeSetup
    {
        #region theme-setup
        // Layer the game's palette over the built-in theme.
        public static MonoGameIcyHost Extend(Game game) =>
            game.UseIcyUI(builder => builder.ConfigureTheme().ExtendDefaultTheme("Themes/MyGame.xml"));
        #endregion

        #region whole-theme
        // Or replace the built-in theme entirely with your own file.
        public static MonoGameIcyHost Replace(Game game) =>
            game.UseIcyUI(builder => builder.ConfigureTheme().UseThemeFile("Themes/Dark.xml"));
        #endregion
    }
}
