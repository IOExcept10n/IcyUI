// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;
using Icy.Data;
using Icy.Rendering;
using Icy.Rendering.Brushes;

namespace Icy.Assets
{
    /// <summary>
    /// Turns an asset path written in markup (<c>Source="Portraits/Warrior.png"</c>) into a texture, or an
    /// <see cref="ImageBrush"/> over it.
    /// </summary>
    /// <remarks>
    /// The path is relative to <see cref="AssetConfiguration.DefaultAssetContext"/>, the same rule merged
    /// dictionaries and page navigation follow. Textures come from <see cref="AssetConfiguration.PathTextures"/>, so
    /// every use of one path shares one texture; each use gets its own <see cref="ImageBrush"/>.
    /// </remarks>
    internal static class TexturePathConverter
    {
        /// <summary>
        /// Registers the path converters for <see cref="ITexture"/>, <see cref="ImageBrush"/> and <see cref="IImage"/>
        /// targets on <paramref name="converter"/>.
        /// </summary>
        /// <param name="converter">The configuration's type converter.</param>
        /// <param name="assets">The assets the paths load through.</param>
        public static void Register(TypeConversionManager converter, AssetConfiguration assets)
        {
            ITexture Load(string path) => assets.PathTextures.Load(assets, path);

            converter.RegisterConverter(new PathConverter<ITexture>(Load));
            converter.RegisterConverter(new PathConverter<ImageBrush>(path => new ImageBrush(Load(path))));
            converter.RegisterConverter(new PathConverter<IImage>(path => new ImageBrush(Load(path))));
        }

        private sealed class PathConverter<T>(Func<string, T> load) : IValueConverter<string, T>
        {
            public T Convert(string value) => load(value);
        }
    }
}
