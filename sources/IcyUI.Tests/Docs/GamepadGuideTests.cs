// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Drawing;
using System.Numerics;
using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Icy.UI.Styles;

namespace Icy.Tests.Docs
{
    /// <summary>
    /// Behaviour of the "Gamepad-friendly UI" guide: the loadout snippet is fully reachable with the D-pad, focus starts
    /// where the code says, dialogs keep focus inside, and gameplay can turn navigation off.
    /// </summary>
    public sealed class GamepadGuideTests
    {
        private static readonly Vector2 Right = new(1, 0);
        private static readonly Vector2 Left = new(-1, 0);
        private static readonly Vector2 Up = new(0, -1);
        private static readonly Vector2 Down = new(0, 1);

        private readonly FakeInputSystem input = new();
        private readonly Canvas canvas;
        private readonly UIElement menu;

        public GamepadGuideTests()
        {
            var configuration = new IcyConfiguration(input, new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()).UseDefaultTheme();
            canvas = new Canvas(configuration) { IsVisible = true, IsInputEnabled = true };
            string markup = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "DocsSnippets", "guides", "gamepad", "gamepad-menu.xml"));
            menu = new MarkupLoader(configuration).Load(markup, "gamepad-menu.xml");
            menu.DataContext = new DocsSnippetViewModel();
            canvas.Add(menu);
            canvas.Render();
        }

        [Fact]
        public void TheDPad_ReachesEveryControl()
        {
            string[] names = ["Sword", "Bow", "Shield", "Potion", "Easy", "Normal", "Hard", "Subtitles", "Difficulty", "Start"];
            UIElement first = menu.FindRequiredControl<Button>("Sword");
            var reached = new HashSet<UIElement> { first };
            var queue = new Queue<UIElement>([first]);

            while (queue.Count > 0)
            {
                UIElement current = queue.Dequeue();
                foreach (Vector2 direction in new[] { Right, Left, Up, Down })
                {
                    canvas.Focus(current);
                    input.Events.Navigation.RaiseFocusChanging(direction);
                    if (canvas.FocusedElement is { } next && reached.Add(next))
                        queue.Enqueue(next);
                }
            }

            foreach (string name in names)
            {
                UIElement control = menu.FindRequiredControl<UIElement>(name);
                Assert.True(reached.Any(e => IsSelfOrDescendant(e, control)), $"The D-pad never reaches '{name}'.");
            }
        }

        [Fact]
        public void FocusingTheFirstControl_ShowsTheFocusedLook()
        {
            var sword = menu.FindRequiredControl<Button>("Sword");
            var bow = menu.FindRequiredControl<Button>("Bow");

            canvas.Focus(sword);
            canvas.Render();

            Assert.Same(sword, canvas.FocusedElement);
            Assert.Equal(unchecked((int)0xFF3C78D8), Assert.IsType<SolidColorBrush>(sword.BorderBrush).Color.ToArgb());
            Assert.NotEqual(unchecked((int)0xFF3C78D8), Assert.IsType<SolidColorBrush>(bow.BorderBrush).Color.ToArgb());
        }

        [Fact]
        public void ADialog_KeepsFocusInsideItself_AndGivesItBack()
        {
            var start = menu.FindRequiredControl<Button>("Start");
            canvas.Focus(start);

            Task<DialogResult> result = MessageBox.ShowAsync(canvas, "Quit?", DialogButtons.YesNo);
            canvas.Render();
            foreach (Vector2 direction in new[] { Down, Right, Down, Left, Up })
            {
                input.Events.Navigation.RaiseFocusChanging(direction);
                Assert.False(IsSelfOrDescendant(canvas.FocusedElement, menu), $"Focus left the dialog after {direction}.");
            }

            input.Events.Navigation.RaiseCloseModal();
            canvas.Render();

            Assert.True(result.IsCompleted);
            Assert.Equal(DialogResult.None, result.Result);
            Assert.Same(start, canvas.FocusedElement);
        }

        [Fact]
        public void TurningNavigationOff_StopsTheDPadMovingFocus()
        {
            var sword = menu.FindRequiredControl<Button>("Sword");
            canvas.Focus(sword);

            canvas.IsKeyboardNavigationEnabled = false;
            input.Events.Navigation.RaiseFocusChanging(Right);
            Assert.Same(sword, canvas.FocusedElement);

            canvas.IsKeyboardNavigationEnabled = true;
            input.Events.Navigation.RaiseFocusChanging(Right);
            Assert.Same(menu.FindRequiredControl<Button>("Bow"), canvas.FocusedElement);
        }

        [Fact]
        public void Show_FocusesStart_WithoutDrawingAFrame()
        {
            // The guide's code runs from game code (for example a script's Start), where the engine can't draw yet.
            var render = new FakeRenderContext();
            var configuration = new IcyConfiguration(input, new AssetConfiguration(AssetContext.ApplicationContext), render, new ReflectionConfiguration()).UseDefaultTheme();
            var freshCanvas = new Canvas(configuration) { IsVisible = true, IsInputEnabled = true };
            string markup = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "DocsSnippets", "guides", "gamepad", "gamepad-menu.xml"));
            UIElement page = new MarkupLoader(configuration).Load(markup, "gamepad-menu.xml");
            page.DataContext = new DocsSnippetViewModel();

            MyGame.Gamepad.GamepadMenu.Show(freshCanvas, page);

            Assert.Equal(0, render.BeginCount);
            Assert.Same(page.FindRequiredControl<Button>("Start"), freshCanvas.FocusedElement);
        }

        private static bool IsSelfOrDescendant(UIElement? element, UIElement ancestor)
        {
            for (UIElement? e = element; e != null; e = e.Parent)
            {
                if (ReferenceEquals(e, ancestor))
                    return true;
            }

            return false;
        }
    }
}
