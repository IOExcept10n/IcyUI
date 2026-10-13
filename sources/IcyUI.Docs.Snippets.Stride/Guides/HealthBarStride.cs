// Stride code for docfx/*/guides/custom-controls.md.
namespace MyGame.Controls
{
    #region register
    using Icy.Configuration;
    using Icy.Stride.Configuration;
    using Stride.Engine;

    /// <summary>Add this script to an entity in your scene: it sets up IcyUI with the game's theme and the HealthBar.</summary>
    public class HudStartup : StartupScript
    {
        public override void Start()
        {
            // The theme file names HealthBar with a clr-namespace prefix, so it loads while the configuration is built.
            StrideIcyHost ui = Game.UseIcyUI(builder => builder.ConfigureTheme().ExtendDefaultTheme("Themes/MyGame.xml"));

            // Optional: lets pages write <HealthBar/> without an xmlns prefix.
            ui.Configuration.Types.Markup.RegisterShortName<HealthBar>();

            ui.MainCanvas.LoadMarkup("Hud.xml", new HudViewModel());
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
