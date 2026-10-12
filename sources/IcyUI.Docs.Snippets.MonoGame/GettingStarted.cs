// Examples for docfx/*/getting-started/monogame.md and first-screen.md. Each #region is one example on a page.
namespace MyGame.GameExample
{
    #region game
    using Icy.MonoGame.Configuration;
    using Icy.UI.Controls;
    using Microsoft.Xna.Framework;

    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            MonoGameIcyHost ui = this.UseIcyUI();
            ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });

            base.Initialize();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime); // IcyUI draws here, on top of what you drew above.
        }
    }
    #endregion
}

namespace MyGame.FirstScreenExample
{
    #region first-screen
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Icy.MonoGame.Configuration;
    using Microsoft.Xna.Framework;

    /// <summary>The menu's data: what the markup's {Binding} expressions read.</summary>
    public sealed class MenuViewModel : ObservableObject
    {
        private string title = "My Game";

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public ICommand PlayCommand { get; } = new RelayCommand(() => { /* Start the game. */ });

        public ICommand QuitCommand { get; } = new RelayCommand(() => { /* Exit. */ });
    }

    public class Game1 : Game
    {
        public Game1()
        {
            _ = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // Loads Menu.xml (copied to the output folder) onto the main canvas.
            this.UseIcyUI().MainCanvas.LoadMarkup("Menu.xml", new MenuViewModel());
            base.Initialize();
        }
    }
    #endregion
}
