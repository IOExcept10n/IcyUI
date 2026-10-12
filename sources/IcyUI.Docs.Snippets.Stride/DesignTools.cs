// Examples for docfx/*/concepts/design-tools.md. Each #region is one example on a page.
namespace MyGame.DesignTools
{
    using Icy.Design;
    using Icy.Design.Editor;
    using Icy.UI;

    public static class DesignToolsExamples
    {
        #region attach-editor
        // Call it before the first screen loads, inside #if DEBUG: pages loaded earlier can't be edited.
        public static void AttachEditor(IIcyHost ui)
        {
            DesignSession design = DesignSession.Attach(ui.Configuration);
            if (DesignSession.FindSourceRoot("MyGame.csproj", "Assets/UI") is { } root)
                design.UseSourceRoot(root);               // Save writes your project's files, not the build output's
            EditorOverlay.Attach(ui.MainCanvas, design);  // F4 shows and hides the editor
        }
        #endregion

        #region workspace
        // The same editor as a page, for a debug menu.
        public static void ShowEditorPage(Icy.UI.Controls.ContentControl debugPage, DesignSession design)
        {
            debugPage.Content = new EditorWorkspace { Design = design };
        }
        #endregion
    }
}
