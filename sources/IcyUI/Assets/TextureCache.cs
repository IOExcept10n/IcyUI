// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Configuration;
using Icy.Rendering;

namespace Icy.Assets
{
    /// <summary>
    /// Textures loaded by path, one per asset context and path, owned until <see cref="Dispose"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="AssetResolver"/> caches only inside a scope, and markup loads outside one: without this cache every
    /// instantiation of a template with <c>Source="..."</c> (a pooled list item, say) would import a new texture.
    /// </remarks>
    internal sealed class TextureCache : IDisposable
    {
        private readonly Dictionary<(IAssetContext Context, string Path), ITexture> textures = [];
        private readonly Lock gate = new();

        /// <summary>
        /// Gets the texture at <paramref name="path"/> relative to <see cref="AssetConfiguration.DefaultAssetContext"/>,
        /// loading it on first use.
        /// </summary>
        /// <param name="assets">The assets to load through.</param>
        /// <param name="path">The texture's path.</param>
        /// <returns>The texture, the same instance for every call with this context and path.</returns>
        public ITexture Load(AssetConfiguration assets, string path)
        {
            var key = (assets.DefaultAssetContext, path);
            lock (gate)
            {
                if (!textures.TryGetValue(key, out ITexture? texture))
                {
                    texture = assets.Load<ITexture>(path);
                    textures.Add(key, texture);
                }

                return texture;
            }
        }

        /// <summary>
        /// Releases every texture: unloads it from a platform context (engine content), disposes it otherwise.
        /// </summary>
        public void Dispose()
        {
            lock (gate)
            {
                foreach (((IAssetContext context, string path), ITexture texture) in textures)
                {
                    if (context is IPlatformAssetReference platform)
                        platform.Unload(path);
                    else
                        texture.Dispose();
                }

                textures.Clear();
            }
        }
    }
}
