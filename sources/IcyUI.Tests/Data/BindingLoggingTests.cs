using CommunityToolkit.Mvvm.ComponentModel;
using Icy.Configuration;
using Icy.Markup;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Icy.Tests.Rendering;
using Icy.UI.Controls;

namespace Icy.Tests.Data
{
    public class BindingLoggingTests
    {
        private static TextBlock Load(string path, CapturingLoggerFactory log)
        {
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureLogging(log)
                .Build();
            return (TextBlock)new MarkupLoader(configuration).Load(
                $"<TextBlock xmlns=\"https://icyui.dev/markup\" Text=\"{{Binding {path}}}\"/>", "x.xml");
        }

        [Fact]
        public void MissingMember_WarnsOncePerBinding()
        {
            var log = new CapturingLoggerFactory();
            TextBlock text = Load("Titel", log);
            var source = new TitleSource();

            text.DataContext = source;
            source.Title = "Pause";
            text.DataContext = new TitleSource();

            CapturedLog entry = Assert.Single(log.Entries, e => e.EventId.Id == 2001);
            Assert.Contains("Titel", entry.Message);
            Assert.Contains(nameof(TitleSource), entry.Message);
        }

        [Fact]
        public void ExistingMember_DoesNotWarn()
        {
            var log = new CapturingLoggerFactory();
            TextBlock text = Load("Title", log);

            text.DataContext = new TitleSource();

            Assert.Equal("Play", text.Text);
            Assert.DoesNotContain(log.Entries, e => e.EventId.Id == 2001);
        }

        [Fact]
        public void NoDataContext_DoesNotWarn()
        {
            var log = new CapturingLoggerFactory();
            Load("Titel", log);

            Assert.DoesNotContain(log.Entries, e => e.EventId.Id == 2001);
        }

        public sealed class TitleSource : ObservableObject
        {
            private string title = "Play";

            public string Title
            {
                get => title;
                set => SetProperty(ref title, value);
            }
        }
    }
}
