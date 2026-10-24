// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Assets;
using Icy.Assets.Importers;
using Icy.Configuration;
using Icy.Markup;
using Icy.Rendering;
using Icy.Rendering.Brushes;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Icy.UI.Controls;
using Xunit;

namespace Icy.Tests.Markup
{
    /// <summary>
    /// An image property set to a path in markup (<c>&lt;Image Source="Portraits/Warrior.png"/&gt;</c>) loads the
    /// texture through the asset pipeline, relative to the default asset context.
    /// </summary>
    public sealed class ImageSourcePathTests : IDisposable
    {
        private readonly string root = Path.Combine(Path.GetTempPath(), "IcyUI.Tests", Guid.NewGuid().ToString("N"));
        private readonly RecordingTextureImporter textures = new();
        private readonly IcyConfiguration configuration;

        public ImageSourcePathTests()
        {
            Directory.CreateDirectory(Path.Combine(root, "Portraits"));
            File.WriteAllBytes(Path.Combine(root, "Portraits", "Warrior.png"), [0]);
            configuration = new IcyConfiguration(
                new FakeInputSystem(),
                new AssetConfiguration(new AssetContext(root + Path.DirectorySeparatorChar)),
                new FakeRenderContext(),
                new ReflectionConfiguration());
            configuration.Assets.AssetResolver.RegisterImporter(textures);
        }

        public void Dispose()
        {
            configuration.Assets.Dispose();
            Directory.Delete(root, recursive: true);
        }

        [Fact]
        public void ImageSource_SetToAPath_LoadsTheTextureThroughTheAssetPipeline()
        {
            var image = (Image)new MarkupLoader(configuration).LoadObject("""<Image Source="Portraits/Warrior.png"/>""");

            var brush = Assert.IsType<ImageBrush>(image.Source);
            Assert.Same(textures.Imported.Single(), brush.Source);
        }

        [Fact]
        public void TwoImagesWithTheSamePath_ShareOneTexture()
        {
            var panel = (StackPanel)new MarkupLoader(configuration).LoadObject(
                """
                <StackPanel>
                  <Image Source="Portraits/Warrior.png"/>
                  <Image Source="Portraits/Warrior.png"/>
                </StackPanel>
                """);

            ITexture[] loaded = panel.Children.Cast<Image>().Select(x => ((ImageBrush)x.Source!).Source).ToArray();
            Assert.Single(textures.Imported);
            Assert.Same(loaded[0], loaded[1]);
        }

        [Fact]
        public void ImageSource_APathThatDoesNotExist_ThrowsMarkupExceptionAtTheElement()
        {
            MarkupException ex = Assert.Throws<MarkupException>(() => new MarkupLoader(configuration).LoadObject(
                """
                <StackPanel>
                  <Image Source="Portraits/Missing.png"/>
                </StackPanel>
                """));

            Assert.Equal(2, ex.Line);
            Assert.Contains("Portraits/Missing.png", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void DisposingTheAssets_DisposesTexturesLoadedFromPaths()
        {
            new MarkupLoader(configuration).LoadObject("""<Image Source="Portraits/Warrior.png"/>""");

            configuration.Assets.Dispose();

            Assert.True(textures.Imported.Single().IsDisposed);
        }

        private sealed class RecordingTextureImporter : IAssetImporter<ITexture>
        {
            public List<TrackedTexture> Imported { get; } = [];

            public bool CanRead(string? format) => format == "image/png";

            public ITexture Import(Stream stream, IImportContext importContext)
            {
                var texture = new TrackedTexture();
                Imported.Add(texture);
                return texture;
            }
        }

        private sealed class TrackedTexture : ITexture
        {
            public bool IsDisposed { get; private set; }

            public System.Drawing.Size Size { get; } = new(1, 1);

            public void Dispose() => IsDisposed = true;

            public void GetTextureData<TColor>(System.Drawing.Rectangle? region, TColor[] buffer, int startIndex, int elementCount)
                where TColor : struct
            {
            }

            public void SetTextureData<TColor>(System.Drawing.Rectangle? region, TColor[] buffer, int startIndex, int elementCount)
                where TColor : struct
            {
            }
        }
    }
}
