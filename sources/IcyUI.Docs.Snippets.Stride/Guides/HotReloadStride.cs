// Stride code for docfx/*/guides/hot-reload-and-editor.md.
namespace MyGame.HotReload
{
    #region design-setup
    using Icy.Stride.Configuration;
    using Icy.UI.Controls;
    using Stride.Engine;

    public class MenuStartup : StartupScript
    {
#if DEBUG
        private DevTools? tools;
#endif

        public override void Start()
        {
            StrideIcyHost ui = Game.UseIcyUI();
            var viewModel = new MenuViewModel();

#if DEBUG
            tools = new DevTools(ui);
            _ = new ReloadablePage(ui, tools.Design, "Menu.xml", viewModel);
#else
            ui.MainCanvas.LoadMarkup("Menu.xml", viewModel);
#endif
        }

#if DEBUG
        public override void Cancel() => tools?.Dispose();
#endif
    }
    #endregion

    /// <summary>The page's data.</summary>
    public sealed class MenuViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public string Title => "My Game";
    }
}
