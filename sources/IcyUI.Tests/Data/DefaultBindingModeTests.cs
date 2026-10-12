using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Assets;
using Icy.Configuration;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Data
{
    /// <summary>
    /// A binding without Mode is one-way, except on the properties the player edits, which write back by default.
    /// </summary>
    public class DefaultBindingModeTests
    {
        public sealed class Settings : ObservableObject
        {
            private string name = "Player 1";
            private float volume = 70;
            private bool subtitles;
            private int level;
            private bool advanced;

            public string Name { get => name; set => SetProperty(ref name, value); }

            public float Volume { get => volume; set => SetProperty(ref volume, value); }

            public bool Subtitles { get => subtitles; set => SetProperty(ref subtitles, value); }

            public int Level { get => level; set => SetProperty(ref level, value); }

            public bool Advanced { get => advanced; set => SetProperty(ref advanced, value); }

            public IReadOnlyList<string> Levels { get; } = ["Forest", "Caves", "Castle"];
        }

        private static T Load<T>(string markup, Settings settings)
            where T : UIElement
        {
            var element = (T)new MarkupLoader(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()))
                .Load(markup.Replace("<X ", "<", StringComparison.Ordinal).Replace(" ns ", " xmlns=\"https://icyui.dev/markup\" ", StringComparison.Ordinal), "x.xml");
            element.DataContext = settings;
            return element;
        }

        [Fact]
        public void TextBlockText_WithoutMode_DoesNotWriteBack()
        {
            var settings = new Settings();
            var label = Load<TextBlock>("<X TextBlock ns Text=\"{Binding Name}\"/>", settings);

            label.Text = "Changed on screen";

            Assert.Equal("Player 1", settings.Name);
        }

        [Fact]
        public void TextBoxText_WithoutMode_WritesBack()
        {
            var settings = new Settings();
            var box = Load<TextBox>("<X TextBox ns Text=\"{Binding Name}\"/>", settings);

            box.Text = "Ivan";

            Assert.Equal("Ivan", settings.Name);
        }

        [Fact]
        public void SliderValue_WithoutMode_WritesBack()
        {
            var settings = new Settings();
            var slider = Load<Slider>("<X Slider ns Minimum=\"0\" Maximum=\"100\" Value=\"{Binding Volume}\"/>", settings);

            slider.Value = 25;

            Assert.Equal(25f, settings.Volume);
        }

        [Fact]
        public void CheckBoxIsChecked_WithoutMode_WritesBack()
        {
            var settings = new Settings();
            var checkBox = Load<CheckBox>("<X CheckBox ns IsChecked=\"{Binding Subtitles}\"/>", settings);

            checkBox.IsChecked = true;

            Assert.True(settings.Subtitles);
        }

        [Fact]
        public void ListBoxSelectedIndex_WithoutMode_WritesBack()
        {
            var settings = new Settings();
            var listBox = Load<ListBox>("<X ListBox ns ItemsSource=\"{Binding Levels}\" SelectedIndex=\"{Binding Level}\"/>", settings);

            listBox.SelectedIndex = 2;

            Assert.Equal(2, settings.Level);
        }

        [Fact]
        public void ExpanderIsExpanded_WithoutMode_WritesBack()
        {
            var settings = new Settings();
            var expander = Load<Expander>("<X Expander ns IsExpanded=\"{Binding Advanced}\"/>", settings);

            expander.IsExpanded = true;

            Assert.True(settings.Advanced);
        }
    }
}
