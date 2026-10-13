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
    public class TextContentConversionTests
    {
        public sealed class TitleSource : ObservableObject
        {
            private string title = "Play";

            public string Title
            {
                get => title;
                set => SetProperty(ref title, value);
            }
        }

        private static Button LoadButton(string attributes) =>
            (Button)new MarkupLoader(new IcyConfiguration(new FakeInputSystem(), new AssetConfiguration(AssetContext.ApplicationContext), new FakeRenderContext(), new ReflectionConfiguration()))
                .Load($"<Button xmlns=\"https://icyui.dev/markup\" {attributes}/>", "x.xml");

        [Fact]
        public void StringContent_InCode_BecomesATextBlock()
        {
            var button = new Button { Content = "Play" };

            Assert.Equal("Play", Assert.IsType<TextBlock>(button.Content).Text);
        }

        [Fact]
        public void StringChild_AddsATextBlock()
        {
            var panel = new StackPanel();

            panel.Children.Add("Score: 0");

            Assert.Equal("Score: 0", Assert.IsType<TextBlock>(Assert.Single(panel.Children)).Text);
        }

        [Fact]
        public void NullString_ConvertsToNull()
        {
            var button = new Button { Content = "Play" };
            string? none = null;

            button.Content = none;

            Assert.Null(button.Content);
        }

        [Fact]
        public void StringAttribute_InMarkup_BecomesATextBlock()
        {
            Button button = LoadButton("Content=\"Play\"");

            Assert.Equal("Play", Assert.IsType<TextBlock>(button.Content).Text);
        }

        [Fact]
        public void StringBinding_FollowsTheSource()
        {
            Button button = LoadButton("Content=\"{Binding Title}\"");
            var source = new TitleSource();

            button.DataContext = source;
            Assert.Equal("Play", Assert.IsType<TextBlock>(button.Content).Text);

            source.Title = "Pause";
            Assert.Equal("Pause", Assert.IsType<TextBlock>(button.Content).Text);
        }
    }
}
