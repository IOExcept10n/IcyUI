// Examples for docfx/*/getting-started/stride.md and first-screen.md. Each #region is one example on a page.
namespace MyGame.StartupScriptExample
{
    #region startup-script
    using Icy.Stride.Configuration;
    using Icy.UI.Controls;
    using Stride.Engine;

    /// <summary>Add this script to any entity in your startup scene: it sets up IcyUI and shows a button.</summary>
    public class IcyUIStartup : StartupScript
    {
        public override void Start()
        {
            StrideIcyHost ui = Game.UseIcyUI();
            ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });
        }
    }
    #endregion
}

namespace MyGame.CodeOnlyExample
{
    #region code-only
    using Icy.Stride.Configuration;
    using Icy.UI.Controls;
    using Stride.Engine;

    /// <summary>A code-only game (no Game Studio project): set up IcyUI once the scene system exists.</summary>
    public class MyGame : Game
    {
        protected override void BeginRun()
        {
            base.BeginRun();

            // Set up your graphics compositor and scene here, before UseIcyUI().
            StrideIcyHost ui = this.UseIcyUI();
            ui.MainCanvas.Add(new Button { Content = "Hello, IcyUI" });
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
    using Icy.Stride.Configuration;
    using Stride.Engine;

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

    /// <summary>Replaces IcyUIStartup: loads Menu.xml (copied to the output folder) onto the main canvas.</summary>
    public class MenuStartup : StartupScript
    {
        public override void Start() =>
            Game.UseIcyUI().MainCanvas.LoadMarkup("Menu.xml", new MenuViewModel());
    }
    #endregion
}
