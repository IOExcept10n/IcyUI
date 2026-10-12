using System.Drawing;
using Icy.Configuration;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Icy.UI.Styles;

namespace Icy.Tests.UI.Styles
{
    /// <summary>
    /// The default theme's visual states, as players see them: combinations of states, not one state at a time.
    /// </summary>
    public class DefaultThemeStateTests
    {
        private static readonly Color FocusBorder = Color.FromArgb(unchecked((int)0xFF3C78D8));
        private static readonly Color ButtonHover = Color.FromArgb(unchecked((int)0xFF46465A));
        private static readonly Color ButtonPress = Color.FromArgb(unchecked((int)0xFF2E2E38));
        private static readonly Color Selected = Color.FromArgb(unchecked((int)0xFF3C78D8));

        [Fact]
        public void Button_HoveredWhileFocused_KeepsFocusBorderAndShowsHover()
        {
            var button = Attach(new Button());

            button.ControlState = ControlState.Focused | ControlState.Hovered;

            Assert.Equal(ButtonHover, ColorOf(button.Background));
            Assert.Equal(FocusBorder, ColorOf(button.BorderBrush));
        }

        [Fact]
        public void Button_MousePress_ShowsPressedOverHovered()
        {
            var button = Attach(new Button());

            button.ControlState = ControlState.Hovered | ControlState.Pressed;

            Assert.Equal(ButtonPress, ColorOf(button.Background));
        }

        [Fact]
        public void SelectorItem_SelectedAndHighlighted_StaysSelectedColor()
        {
            var item = Attach(new SelectorItem());

            item.ControlState = ControlState.Selected | ControlState.Highlighted;
            Assert.Equal(Selected, ColorOf(item.Background));

            item.ControlState = ControlState.Selected | ControlState.Highlighted | ControlState.Hovered;
            Assert.Equal(Selected, ColorOf(item.Background));
        }

        [Fact]
        public void SelectorItem_SelectedAndDisabled_ShowsBoth()
        {
            var item = Attach(new SelectorItem());

            item.ControlState = ControlState.Selected | ControlState.Disabled;

            Assert.Equal(Selected, ColorOf(item.Background));
            Assert.Equal(0.5f, item.Opacity);
        }

        [Fact]
        public void UserStyle_CommonStatesGroup_ReplacesThemeGroupInsteadOfMixingWithIt()
        {
            var hover = Color.FromArgb(unchecked((int)0xFF5A87E6));
            var state = new VisualState(ControlState.Hovered) { Name = "Hovered" };
            state.Setters["Background"] = new SolidColorBrush(hover);
            var group = new VisualStateGroup { Name = "CommonStates" };
            group.States.Add(state);
            var style = new Style(typeof(Button));
            style.StateGroups.Add(group);
            var button = Attach(new Button { Style = style });

            // The theme's Pressed state is gone with its group: a press keeps the user's hover color.
            button.ControlState = ControlState.Hovered | ControlState.Pressed;
            Assert.Equal(hover, ColorOf(button.Background));

            // The theme's FocusStates group isn't replaced, so focus still shows.
            button.ControlState = ControlState.Hovered | ControlState.Focused;
            Assert.Equal(FocusBorder, ColorOf(button.BorderBrush));
        }

        private static T Attach<T>(T element)
            where T : UIElement
        {
            var builder = new IcyConfigurationBuilder();
            builder.ConfigureRendering(new FakeRenderContext())
                   .ConfigureInput(new FakeInputSystem())
                   .ConfigureTypes()
                   .ConfigureAssets();
            var canvas = new Canvas(builder.Build().UseDefaultTheme()) { IsInputEnabled = true, IsVisible = true };
            canvas.Add(element);
            return element;
        }

        private static Color ColorOf(IBrush? brush) => Assert.IsType<SolidColorBrush>(brush).Color;
    }
}
