// Engine-neutral code for docfx/*/guides/gamepad.md. Each #region is one example on the page. This file is also compiled
// into the MonoGame snippets. It must not reference an engine.
namespace MyGame.Gamepad
{
    using System.Threading.Tasks;
    using Icy.UI;
    using Icy.UI.Controls;

    /// <summary>Focus handling of a gamepad-operated menu.</summary>
    public static class GamepadMenu
    {
        #region focus-on-open
        /// <summary>Shows a menu page and puts the focus on its first button, so the D-pad has somewhere to start.</summary>
        public static void Show(Canvas canvas, UIElement menu)
        {
            canvas.Add(menu);
            canvas.Render();   // the first layout pass: a control that hasn't been laid out can't be focused
            canvas.Focus(menu.FindRequiredControl<Button>("Start"));
        }
        #endregion

        #region confirm-dialog
        /// <summary>A dialog is a focus scope: the D-pad stays inside it, B closes it, and the focus returns to the menu.</summary>
        public static async Task<bool> ConfirmQuitAsync(Canvas canvas)
        {
            DialogResult result = await MessageBox.ShowAsync(canvas, "Quit to desktop?", DialogButtons.YesNo);
            return result == DialogResult.Yes;
        }
        #endregion

        #region gameplay-switch
        /// <summary>While the player controls the game, the D-pad and sticks belong to gameplay, not to the menu focus.</summary>
        public static void SetGameplay(Canvas canvas, bool playing) => canvas.IsKeyboardNavigationEnabled = !playing;
        #endregion
    }
}
