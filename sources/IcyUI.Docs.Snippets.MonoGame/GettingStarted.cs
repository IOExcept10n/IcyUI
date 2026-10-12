// Examples for docfx/*/getting-started/monogame.md and first-screen.md. Each #region is one example on a page.
namespace MyGame.GameExample
{
    #region game
    using Icy.Configuration;
    using Icy.MonoGame.Configuration;
    using Icy.UI;
    using Microsoft.Xna.Framework;

    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private Canvas canvas = null!;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            MonoGameIcyHost ui = this.UseIcyUI();
            canvas = ui.MainCanvas;

            base.Initialize();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }
    }
    #endregion
}

namespace MyGame.FirstScreenExample
{
    #region first-screen
    using System;
    using System.IO;
    using Icy.Configuration;
    using Icy.Markup;
    using Icy.UI;

    public static class MainMenu
    {
        /// <summary>Loads Menu.xml (copied to the output folder) and shows it on the canvas.</summary>
        public static void Show(Canvas canvas, IcyConfiguration configuration, object viewModel)
        {
            UIElement menu = new MarkupLoader(configuration).Load(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Menu.xml")), "Menu.xml");
            menu.DataContext = viewModel;
            canvas.Add(menu);
        }
    }
    #endregion
}
