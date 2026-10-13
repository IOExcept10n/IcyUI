// MonoGame code for docfx/*/guides/game-menu.md.
namespace MyGame.Menus
{
    #region pause-toggle
    using Icy.MonoGame.Configuration;
    using Microsoft.Xna.Framework;

    public class MenuGame : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private MenuController menu = null!;

        public MenuGame()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            MonoGameIcyHost ui = this.UseIcyUI();
            menu = new MenuController(ui);
            menu.QuitRequested += (_, _) => Exit();

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            // UI time: the Settings countdown must keep running while the game is paused.
            menu.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (menu.IsPlaying && !menu.IsPaused)
            {
                // Your gameplay update goes here.
            }

            base.Update(gameTime);
        }
    }
    #endregion
}
