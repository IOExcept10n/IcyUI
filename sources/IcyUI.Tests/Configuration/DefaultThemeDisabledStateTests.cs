using Icy.Assets;
using Icy.Configuration;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.Configuration
{
    /// <summary>
    /// Every interactive control in the default theme looks disabled while it is (ICY-007): the theme's
    /// <c>EnabledStates</c> group dims it.
    /// </summary>
    public class DefaultThemeDisabledStateTests
    {
        public static TheoryData<Type> InteractiveControls() =>
        [
            typeof(Button),
            typeof(ToggleButton),
            typeof(CheckBox),
            typeof(Slider),
            typeof(ExpanderHeader),
            typeof(Dropdown),
            typeof(ComboBox),
            typeof(TextBox),
            typeof(ColorPickerButton),
            typeof(SelectorItem),
            typeof(TreeViewItem),
        ];

        [Theory]
        [MemberData(nameof(InteractiveControls))]
        public void DisabledControl_IsDimmed_AndRestoredWhenEnabledAgain(Type type)
        {
            var canvas = new Canvas(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()).UseDefaultTheme());
            var control = (UIElement)Activator.CreateInstance(type)!;
            canvas.Add(control);

            control.IsEnabled = false;
            Assert.Equal(0.5f, control.Opacity);

            control.IsEnabled = true;
            Assert.Equal(1f, control.Opacity);
        }
    }
}
