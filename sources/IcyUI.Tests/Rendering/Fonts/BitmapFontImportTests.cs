using Icy.Assets;
using Icy.Assets.Importers;
using Icy.Configuration;
using Icy.Rendering;
using Icy.Rendering.Fonts;
using Icy.Tests.Input;

namespace Icy.Tests.Rendering.Fonts
{
    /// <summary>
    /// A bitmap (BMFont) font imported through the font system: its pages load from the font's own folder, and text
    /// finds it at its own size only.
    /// </summary>
    public sealed class BitmapFontImportTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "IcyUI.Tests", Guid.NewGuid().ToString("N"));

        public BitmapFontImportTests()
        {
            // TestFont.fnt is "Arial", size 24, with one page: arial24_00.png.
            Directory.CreateDirectory(Path.Combine(root, "Fonts"));
            File.Copy("Resources/TestFont.fnt", Path.Combine(root, "Fonts", "TestFont.fnt"));
            File.WriteAllBytes(Path.Combine(root, "Fonts", "arial24_00.png"), [0]);
        }

        public void Dispose() => Directory.Delete(root, recursive: true);

        [Fact]
        public void ImportFont_FromASubfolder_LoadsPagesNextToTheFontFile()
        {
            var textures = new RecordingTextureImporter();
            IcyConfiguration configuration = NewConfiguration(textures);

            configuration.Fonts.ImportFont(new AssetContext(root + Path.DirectorySeparatorChar), "Fonts/TestFont.fnt");

            Assert.Equal(["Fonts/arial24_00.png"], textures.Requested);
        }

        [Fact]
        public void ImportedBitmapFont_IsFoundAtItsOwnSizeOnly()
        {
            IcyConfiguration configuration = NewConfiguration(new RecordingTextureImporter());
            configuration.Fonts.ImportFont(new AssetContext(root + Path.DirectorySeparatorChar), "Fonts/TestFont.fnt");

            Assert.NotNull(configuration.Fonts.ResolveTextFont("Arial", 24, FontStyle.Regular));
            Assert.Null(configuration.Fonts.ResolveTextFont("Arial", 16, FontStyle.Regular));
        }

        private static IcyConfiguration NewConfiguration(RecordingTextureImporter textures) =>
            new IcyConfigurationBuilder()
                .ConfigureRendering(new FakeRenderContext())
                .ConfigureInput(new FakeInputSystem())
                .ConfigureAssets()
                .AddBitmapFontSupport()
                .AddImporter(textures)
                .Build();

        private sealed class RecordingTextureImporter : IAssetImporter<ITexture>
        {
            public List<string?> Requested { get; } = [];

            public bool CanRead(string? format) => format == "image/png";

            public ITexture Import(Stream stream, IImportContext importContext)
            {
                Requested.Add(importContext.ResourceName);
                return new FakeRenderContext().CreateTexture(1, 1, new int[1]);
            }
        }
    }
}
