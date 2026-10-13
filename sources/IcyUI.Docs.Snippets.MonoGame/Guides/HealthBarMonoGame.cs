// MonoGame code for docfx/*/guides/custom-controls.md.
namespace MyGame.Controls
{
    #region register
    using Icy.Configuration;
    using Icy.MonoGame.Configuration;
    using Microsoft.Xna.Framework;

    public class HudGame : Game
    {
        private readonly GraphicsDeviceManager graphics;

        public HudGame()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // The theme file names HealthBar with a clr-namespace prefix, so it loads while the configuration is built.
            MonoGameIcyHost ui = this.UseIcyUI(builder => builder.ConfigureTheme().ExtendDefaultTheme("Themes/MyGame.xml"));

            // Optional: lets pages write <HealthBar/> without an xmlns prefix.
            ui.Configuration.Types.Markup.RegisterShortName<HealthBar>();

            ui.MainCanvas.LoadMarkup("Hud.xml", new HudViewModel());

            base.Initialize();
        }
    }
    #endregion

    /// <summary>The HUD's data: the markup binds the bar's Value to Health.</summary>
    public sealed class HudViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        private float health = 100;

        public float Health
        {
            get => health;
            set => SetProperty(ref health, value);
        }
    }
}
