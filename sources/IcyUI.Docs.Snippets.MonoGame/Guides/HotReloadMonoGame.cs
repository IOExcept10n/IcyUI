// MonoGame code for docfx/*/guides/hot-reload-and-editor.md.
namespace MyGame.HotReload
{
    #region design-setup
    using Icy.MonoGame.Configuration;
    using Icy.UI.Controls;
    using Microsoft.Xna.Framework;

    public class MenuGame : Game
    {
        private readonly GraphicsDeviceManager graphics;
#if DEBUG
        private DevTools? tools;
#endif

        public MenuGame()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            MonoGameIcyHost ui = this.UseIcyUI();
            var viewModel = new MenuViewModel();

#if DEBUG
            tools = new DevTools(ui);
            _ = new ReloadablePage(ui, tools.Design, "Menu.xml", viewModel);
#else
            ui.MainCanvas.LoadMarkup("Menu.xml", viewModel);
#endif

            base.Initialize();
        }

#if DEBUG
        protected override void UnloadContent()
        {
            tools?.Dispose();
            base.UnloadContent();
        }
#endif
    }
    #endregion

    /// <summary>The page's data.</summary>
    public sealed class MenuViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public string Title => "My Game";
    }
}
