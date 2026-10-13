// Stride code for docfx/*/guides/game-menu.md.
namespace MyGame.Menus
{
    #region pause-toggle
    using Icy.Stride.Configuration;
    using Stride.Engine;

    /// <summary>Add this script to an entity in your scene: it shows the main menu and pauses gameplay behind the pause menu.</summary>
    public class MenuScript : SyncScript
    {
        private MenuController menu = null!;

        public override void Start()
        {
            StrideIcyHost ui = Game.UseIcyUI();
            menu = new MenuController(ui);
            menu.QuitRequested += (_, _) => ((Game)Game).Exit();
        }

        public override void Update()
        {
            // UI time: the Settings countdown must keep running while the game is paused.
            menu.Update((float)Game.UpdateTime.Elapsed.TotalSeconds);

            if (!menu.IsPlaying || menu.IsPaused)
                return;

            // Your gameplay update goes here.
        }
    }
    #endregion
}
