// Engine-neutral code for docfx/*/guides/hot-reload-and-editor.md. Each #region is one example on the page. This file is also
// compiled into IcyUI.Tests (and into the MonoGame snippets), so the guide's behaviour is tested. In a game, DevTools and
// ReloadablePage belong in a file that only Debug builds compile, because they use IcyUI.Design (see the guide).
namespace MyGame.HotReload
{
    using System;
    using CommunityToolkit.Mvvm.Input;
    using Icy.Design;
    using Icy.Design.Editor;
    using Icy.Input.Devices;
    using Icy.Markup;
    using Icy.UI;

    #region dev-tools
    /// <summary>The game's development tools: page editing in a text editor and in the game. Create it before the first screen loads.</summary>
    public sealed class DevTools : IDisposable
    {
        private readonly IDisposable watch;
        private readonly EditorOverlay overlay;

        public DevTools(IIcyHost ui, string? sourceRoot = null)
        {
            // The session sees only the pages loaded after this line.
            Design = DesignSession.Attach(ui.Configuration);

            // Pages load from the build output; Save and the file watcher use your project's copy.
            sourceRoot ??= DesignSession.FindSourceRoot("MyGame.csproj", "Assets/UI");
            if (sourceRoot != null)
                Design.UseSourceRoot(sourceRoot);

            watch = Design.WatchSourceFiles();          // a file saved in your text editor shows in the game
            overlay = EditorOverlay.Attach(ui.MainCanvas, Design);   // F4 shows and hides the in-game editor
        }

        public DesignSession Design { get; }

        public void Dispose()
        {
            overlay.Dispose();
            watch.Dispose();
            Design.Dispose();
        }
    }
    #endregion

    #region reload-page
    /// <summary>
    /// A page that F5 loads again from its document's latest text. Most edits show on the live page at once; an edit inside a
    /// style or a template doesn't, and this shows it.
    /// </summary>
    public sealed class ReloadablePage
    {
        private readonly IIcyHost ui;
        private readonly DesignSession design;
        private readonly string path;
        private readonly object dataContext;
        private UIElement page;

        public ReloadablePage(IIcyHost ui, DesignSession design, string path, object dataContext)
        {
            this.ui = ui;
            this.design = design;
            this.path = path;
            this.dataContext = dataContext;
            page = ui.MainCanvas.LoadMarkup(path, dataContext);
            ui.Configuration.Input.Events.RegisterCommand(new RelayCommand(Reload), new KeyGesture(Keys.F5));
        }

        public UIElement Page => page;

        public void Reload()
        {
            // The document holds the latest text, from the file or from the editor. LoadMarkup(path) would read the build
            // output's older copy.
            DesignDocument document = design.FindDocument(page, out _) ?? throw new InvalidOperationException("The page isn't tracked.");
            UIElement fresh = new MarkupLoader(ui.Configuration).Load(document.Text, path);
            fresh.DataContext = dataContext;

            ui.MainCanvas.Remove(page);
            ui.MainCanvas.Add(fresh);
            page = fresh;
        }
    }
    #endregion
}
