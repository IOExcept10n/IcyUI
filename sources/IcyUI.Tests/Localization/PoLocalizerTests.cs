using System.Globalization;
using Icy.Assets;
using Icy.Configuration;
using Icy.Localization;
using Icy.Tests.Input;
using Icy.Tests.Rendering;

namespace Icy.Tests.Localization
{
    public sealed class PoLocalizerTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "IcyUI.Tests", Guid.NewGuid().ToString("N"));

        public PoLocalizerTests() => Directory.CreateDirectory(Path.Combine(root, "Localization"));

        public void Dispose() => Directory.Delete(root, recursive: true);

        private void Write(string fileName, string body) =>
            File.WriteAllText(Path.Combine(root, "Localization", fileName), body);

        private PoLocalizer Create() =>
            new(new AssetConfiguration(new AssetContext(root + Path.DirectorySeparatorChar)), "Localization", CultureInfo.GetCultureInfo("en"));

        [Fact]
        public void FallsBackFromRegionToLanguageToDefault()
        {
            Write("en.po", "msgid \"A\"\nmsgstr \"a-en\"\nmsgid \"B\"\nmsgstr \"b-en\"\nmsgid \"C\"\nmsgstr \"c-en\"\n");
            Write("ru.po", "msgid \"A\"\nmsgstr \"a-ru\"\nmsgid \"B\"\nmsgstr \"b-ru\"\n");
            Write("ru-RU.po", "msgid \"A\"\nmsgstr \"a-ru-RU\"\n");
            PoLocalizer localizer = Create();
            var culture = CultureInfo.GetCultureInfo("ru-RU");

            Assert.Equal("a-ru-RU", localizer.Localize("A", culture));
            Assert.Equal("b-ru", localizer.Localize("B", culture));
            Assert.Equal("c-en", localizer.Localize("C", culture));
            Assert.Null(localizer.Localize("D", culture));
        }

        [Fact]
        public void GettextUnderscoreFileNames_AreFound()
        {
            Write("pt_BR.po", "msgid \"A\"\nmsgstr \"a-pt-BR\"\n");

            Assert.Equal("a-pt-BR", Create().Localize("A", CultureInfo.GetCultureInfo("pt-BR")));
        }

        [Fact]
        public void EachFileIsReadOnce()
        {
            Write("ru.po", "msgid \"A\"\nmsgstr \"first\"\n");
            PoLocalizer localizer = Create();
            Assert.Equal("first", localizer.Localize("A", CultureInfo.GetCultureInfo("ru")));

            Write("ru.po", "msgid \"A\"\nmsgstr \"second\"\n");

            Assert.Equal("first", localizer.Localize("A", CultureInfo.GetCultureInfo("ru")));
        }

        [Fact]
        public void NoFiles_IsNotAnError()
        {
            Assert.Null(Create().Localize("A", CultureInfo.GetCultureInfo("de")));
        }

        [Fact]
        public void UsePoLocalization_SetsTheLocalizer()
        {
            var builder = new IcyConfigurationBuilder();
            builder.ConfigureRendering(new FakeRenderContext()).ConfigureInput(new FakeInputSystem()).ConfigureTypes();
            builder.ConfigureAssets().UsePoLocalization("Localization", CultureInfo.GetCultureInfo("en"));

            Assert.IsType<PoLocalizer>(builder.Build().Assets.Localizer);
        }
    }
}
