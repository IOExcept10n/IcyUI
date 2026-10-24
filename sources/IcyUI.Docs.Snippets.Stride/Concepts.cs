// Examples for docfx/*/concepts/*.md. Each #region is one example on a page.
namespace MyGame.Concepts
{
    using System;
    using System.Drawing;
    using CommunityToolkit.Mvvm.Input;
    using Icy.Animations;
    using Icy.Configuration;
    using Icy.Input.Devices;
    using Icy.Input.DragDrop;
    using Icy.Stride.Configuration;
    using Icy.UI;
    using Icy.UI.Controls;
    using Microsoft.Extensions.Logging;
    using Stride.Games;

    public static class ConceptsExamples
    {
        #region canvases
        // A second canvas for a HUD, drawn above the main one; a background canvas at index 0 would draw below.
        public static Canvas AddHudCanvas(IIcyHost ui)
        {
            var hud = new Canvas(ui.Configuration);
            ui.Canvases.Add(hud);
            return hud;
        }
        #endregion

        #region tapped
        // A world map under the HUD: where on it did the player click?
        public static void TrackClicks(Border map) =>
            map.Tapped += (_, e) => Console.WriteLine($"Map clicked at {e.GetPosition(map)}");
        #endregion

        #region game-input
        // In your game's update: don't fire when the player clicks a button.
        public static bool ShouldFire(IIcyHost ui, bool mousePressed) => mousePressed && !ui.MainCanvas.IsMouseOverGUI;
        #endregion

        #region configure
        public static StrideIcyHost SetUp(IGame game) => game.UseIcyUI(ui =>
        {
            ui.ConfigureFonts().WithDefaultFontFamily("Noto Sans");                 // instead of Segoe UI
            ui.ConfigureLogging(LogLevel.Information);                             // more than warnings
            ui.ConfigureGestures(gestures => gestures.HoldDelay = TimeSpan.FromSeconds(0.8));
        });
        #endregion

        #region animate
        // Fades a panel in over 0.3 seconds.
        public static Animation FadeIn(UIElement panel) =>
            panel.Animate(Timeline.FromTo(nameof(UIElement.Opacity), TimeSpan.FromSeconds(0.3), 0f, 1f, Easing.EaseOutCubic));
        #endregion

        #region scaling
        // The UI is designed for 1920 x 1080 and scales to fit the window; the player's UI size setting multiplies it.
        public static void DesignFor1080p(IConfigurationBuilder ui, float playerUiScale) =>
            ui.ConfigureScaling(scaling =>
            {
                scaling.Mode = UIScaleMode.ReferenceResolution;
                scaling.ReferenceSize = new Size(1920, 1080);
                scaling.ReferenceFit = ReferenceFit.Fit;
                scaling.UserScale = playerUiScale;
            });
        #endregion

        #region load-asset
        // Loads a markup file from the game's folder without adding it anywhere yet.
        public static UIElement LoadPauseMenu(IIcyHost ui) => ui.Configuration.Assets.Load<UIElement>("Menus/Pause.xml");
        #endregion

        #region debug-keys
        // F1 toggles the layout boxes, F2 the diagnostics panel (frame time, memory, focused element).
        public static void BindDebugKeys(IIcyHost ui)
        {
            void Toggle(string tool)
            {
                if (!ui.MainCanvas.ActiveDebugTools.Remove(tool))
                    ui.MainCanvas.ActiveDebugTools.Add(tool);
            }

            ui.Configuration.Input.Events.RegisterCommand(new RelayCommand(() => Toggle("Bounds")), new KeyGesture(Keys.F1));
            ui.Configuration.Input.Events.RegisterCommand(new RelayCommand(() => Toggle("DiagnosticsHud")), new KeyGesture(Keys.F2));
        }
        #endregion

        #region logging
        // Sends IcyUI's messages to your own logging (Serilog, the console, ...) through any ILoggerProvider.
        public static StrideIcyHost SetUpWithLogging(IGame game, ILoggerProvider provider) =>
            game.UseIcyUI(ui => ui.ConfigureLogging().AddLoggerProvider(provider));
        #endregion
    }

    #region drag-drop
    /// <summary>An inventory slot: drag its item to another slot.</summary>
    public sealed class InventorySlot : Border, IDragSource, IDropTarget
    {
        public string? Item { get; set; }

        public bool TryBeginDrag(Point screenPoint, out object? payload, out UIElement? preview)
        {
            payload = Item;
            preview = Item is null ? null : new TextBlock { Text = Item };
            return Item is not null;
        }

        public bool CanDrop(DragDropSession session) => Item is null && session.Payload is string;

        public void OnDragEnter(DragDropSession session) => Opacity = 0.7f;

        public void OnDragOver(DragDropSession session)
        {
        }

        public void OnDragLeave(DragDropSession session) => Opacity = 1f;

        public void OnDrop(DragDropSession session)
        {
            Item = (string)session.Payload;
            ((InventorySlot)session.Source).Item = null;
            Opacity = 1f;
        }
    }
    #endregion
}
