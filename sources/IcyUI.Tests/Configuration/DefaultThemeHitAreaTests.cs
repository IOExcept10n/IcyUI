using System.Drawing;
using Icy.Configuration;
using Icy.Input.Events;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.Configuration
{
    /// <summary>
    /// Once empty layout passes the pointer through, every interactive control in the default theme must still be hit over
    /// its whole area.
    /// </summary>
    public class DefaultThemeHitAreaTests
    {
        private static (Canvas Canvas, FakeInputSystem Input) CreateCanvas()
        {
            var input = new FakeInputSystem();
            IcyConfiguration config = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext { ViewportSize = new Size(800, 600) })
                .ConfigureInput(input)
                .ConfigureTypes()
                .ConfigureAssets()
                .Build()
                .UseDefaultTheme();
            return (new Canvas(config) { IsInputEnabled = true, IsVisible = true }, input);
        }

        private static IEnumerable<UIElement> SelfAndAncestors(UIElement element)
        {
            for (UIElement? current = element; current != null; current = current.Parent)
                yield return current;
        }

        public static TheoryData<Type> InteractiveControls() =>
        [
            typeof(Button), typeof(ToggleButton), typeof(CheckBox), typeof(Slider), typeof(ExpanderHeader),
            typeof(Dropdown), typeof(ComboBox), typeof(TextBox), typeof(ColorPickerButton),
        ];

        [Theory]
        [MemberData(nameof(InteractiveControls))]
        public void EveryPointOfTheControl_HitsTheControlOrItsTemplate(Type type)
        {
            (Canvas canvas, _) = CreateCanvas();
            var control = (UIElement)Activator.CreateInstance(type)!;
            control.Width = 120;
            control.Height = 32;
            control.HorizontalAlignment = HorizontalAlignment.Left;
            control.VerticalAlignment = VerticalAlignment.Top;
            canvas.Add(control);
            canvas.Render();

            for (int x = 1; x < 120; x += 7)
            {
                for (int y = 1; y < 32; y += 5)
                {
                    UIElement? hit = canvas.HitTest(new Point(x, y));
                    Assert.True(hit != null && SelfAndAncestors(hit).Contains(control), $"{type.Name} missed at ({x},{y})");
                }
            }
        }

        [Fact]
        public void CheckBox_TapInTheGapBetweenBoxAndLabel_TogglesIt()
        {
            (Canvas canvas, FakeInputSystem input) = CreateCanvas();
            var box = new CheckBox { Content = "Sound", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            canvas.Add(box);
            canvas.Render();

            // The box is 16 px wide with an 8 px right margin: x = 20 is in the gap.
            input.Events.Touch.RaiseTap(new TouchInfo(new Point(20, 8), 1));

            Assert.True(box.IsChecked);
        }

        [Fact]
        public void OpenComboBox_PressOnItsListsEdge_DoesNotCloseIt()
        {
            // Light dismiss runs on touch-down: a press on the popup's own border, where no item is, is inside the list.
            (Canvas canvas, FakeInputSystem input) = CreateCanvas();
            var combo = new ComboBox { ItemsSource = new List<object> { "a" }, Width = 150, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            canvas.Add(combo);
            canvas.Render();
            combo.IsOpen = true;
            canvas.Render();
            Rectangle b = canvas.Overlays.Single().ActualBounds;

            input.Events.Touch.RaiseTouchDown(new Point(b.Left, b.Bottom - 1));

            Assert.True(combo.IsOpen);
        }
    }
}
