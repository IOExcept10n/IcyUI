using System.Globalization;
using Icy.Configuration;
using Icy.Localization;
using Icy.Tests.Input;
using Icy.Tests.Logging;
using Icy.Tests.Rendering;
using Icy.UI;
using Icy.UI.Controls;

namespace Icy.Tests.Localization
{
    /// <summary>
    /// <see cref="MessageBox"/> button captions come from the localizer (msgctxt <c>Icy.MessageBox</c>), fall back to
    /// English silently, and can be replaced outright by the game.
    /// </summary>
    public class MessageBoxLocalizationTests
    {
        private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru");

        private static (Canvas Canvas, CapturingLoggerFactory Log) Create()
        {
            var log = new CapturingLoggerFactory();
            IcyConfiguration configuration = new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureLogging(log)
                .Build();
            configuration.Assets.Localizer = new DictionaryLocalizer
            {
                { Russian, "Icy.MessageBox\u0004Yes", "Да" },
                { Russian, "Icy.MessageBox\u0004No", "Нет" },
                { Russian, "Icy.MessageBox\u0004OK", "ОК" },
                { Russian, "Icy.MessageBox\u0004Cancel", "Отмена" },
            };
            return (new Canvas(configuration), log);
        }

        private static string[] Captions(Canvas canvas) =>
            canvas.Overlays.OfType<Dialog>().Single().Content!.EnumerateVisualSubtree().OfType<Button>()
                .Select(b => ((TextBlock)b.Content!).Text).ToArray();

        [Fact]
        public void ShowAsync_TranslatesTheCaptions_InTheConfigurationsCulture()
        {
            var (canvas, _) = Create();
            canvas.Configuration.Culture = Russian;

            _ = MessageBox.ShowAsync(canvas, "Забег будет потерян", DialogButtons.YesNo);

            Assert.Equal(["Да", "Нет"], Captions(canvas));
        }

        [Fact]
        public void ShowAsync_WithoutATranslation_ShowsEnglish_AndLogsNothing()
        {
            var (canvas, log) = Create();
            canvas.Configuration.Culture = CultureInfo.GetCultureInfo("en");

            _ = MessageBox.ShowAsync(canvas, "message", DialogButtons.OKCancel);

            Assert.Equal(["OK", "Cancel"], Captions(canvas));
            Assert.DoesNotContain(log.Entries, e => e.EventId.Id == 4001);
        }

        [Fact]
        public void ShowInputAsync_TranslatesItsButtons()
        {
            var (canvas, _) = Create();
            canvas.Configuration.Culture = Russian;

            _ = MessageBox.ShowInputAsync(canvas, "Имя:");

            Assert.Equal(["ОК", "Отмена"], Captions(canvas));
        }

        [Fact]
        public async Task ShowAsync_WithCaptions_UsesThemOverTheLocalizer()
        {
            var (canvas, _) = Create();
            canvas.Configuration.Culture = Russian;

            Task<DialogResult> task = MessageBox.ShowAsync(
                canvas,
                "Забег будет потерян",
                DialogButtons.OKCancel,
                result => result == DialogResult.OK ? "Выйти" : "Отмена");

            Assert.Equal(["Выйти", "Отмена"], Captions(canvas));
            canvas.Overlays.OfType<Dialog>().Single().Content!.EnumerateVisualSubtree().OfType<Button>().First().OnTap();
            Assert.Equal(DialogResult.OK, await task);
        }
    }
}
