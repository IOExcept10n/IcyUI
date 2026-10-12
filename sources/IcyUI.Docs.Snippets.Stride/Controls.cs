// Examples for docfx/*/controls/*.md. Each #region is one example on a page.
namespace MyGame.ControlsExamples
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Icy.UI;
    using Icy.UI.Controls;

    public static class ControlsExamples
    {
        #region tabs
        // The view model behind <TabControl ItemsSource="{Binding Tabs}" />: each TabItem is a tab's header and its page.
        public static IReadOnlyList<TabItem> CreateSettingsTabs() =>
        [
            new TabItem { Header = "Video", Content = "Resolution: 1920 × 1080" },
            new TabItem { Header = "Audio", Content = "Master volume: 70%" },
            new TabItem { Header = "Controls", Content = "Invert Y axis: off" },
        ];
        #endregion

        #region message-box
        public static async Task<bool> ConfirmQuitAsync(Canvas canvas)
        {
            // Shows a modal message over the canvas and waits for the player's choice; Escape closes it with DialogResult.None.
            DialogResult result = await MessageBox.ShowAsync(canvas, "Quit to the main menu?", DialogButtons.OKCancel);
            return result == DialogResult.OK;
        }
        #endregion
    }
}
