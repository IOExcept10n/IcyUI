// Examples for docfx/*/getting-started/stride.md and first-screen.md. Each #region is one example on a page.
namespace MyGame.StartupScriptExample
{
    #region startup-script
    using Icy.Configuration;
    using Icy.Stride.Configuration;
    using Icy.UI;
    using Icy.UI.Controls;
    using Stride.Engine;

    /// <summary>Add this script to any entity in your startup scene: it sets up IcyUI and shows a button.</summary>
    public class IcyUIStartup : StartupScript
    {
        public override void Start()
        {
            var game = (Game)Game;
            IcyUISceneRenderer overlay = game.UseIcyUI();
            IcyConfiguration configuration = game.GetIcyConfiguration();
            configuration.UseDefaultTheme();

            var canvas = new Canvas(configuration) { IsInputEnabled = true };
            overlay.Canvases.Add(canvas);

            canvas.Add(new Button { Content = new TextBlock { Text = "Hello, IcyUI" } });
        }
    }
    #endregion
}

namespace MyGame.CodeOnlyExample
{
    #region code-only
    using Icy.Configuration;
    using Icy.Stride.Configuration;
    using Icy.UI;
    using Stride.Engine;

    /// <summary>A code-only game (no Game Studio project): set up IcyUI once the scene system exists.</summary>
    public class MyGame : Game
    {
        protected override void BeginRun()
        {
            base.BeginRun();

            // Set up your graphics compositor and scene here, before UseIcyUI().
            IcyUISceneRenderer overlay = this.UseIcyUI();
            IcyConfiguration configuration = this.GetIcyConfiguration();
            configuration.UseDefaultTheme();
            overlay.Canvases.Add(new Canvas(configuration) { IsInputEnabled = true });
        }
    }
    #endregion
}

namespace MyGame.FirstScreenExample
{
    #region first-screen
    using System;
    using System.IO;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Icy.Configuration;
    using Icy.Markup;
    using Icy.UI;

    /// <summary>
    /// The menu's data: what the markup's {Binding} expressions read. ObservableObject raises PropertyChanged, so the
    /// screen updates when Title changes.
    /// </summary>
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

    public static class MainMenu
    {
        /// <summary>Loads Menu.xml (copied to the output folder) and shows it on the canvas.</summary>
        public static void Show(Canvas canvas, IcyConfiguration configuration)
        {
            UIElement menu = new MarkupLoader(configuration).Load(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Menu.xml")), "Menu.xml");
            menu.DataContext = new MenuViewModel();
            canvas.Add(menu);
        }
    }
    #endregion
}
