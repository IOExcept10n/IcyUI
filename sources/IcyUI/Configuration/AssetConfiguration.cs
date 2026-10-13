// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Icy.Assets;
using Icy.Localization;

namespace Icy.Configuration
{
    /// <summary>
    /// Represents configuration for the library assets.
    /// </summary>
    public sealed class AssetConfiguration : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetConfiguration"/> class.
        /// </summary>
        /// <param name="defaultAssetContext">The root UI asset context.</param>
        public AssetConfiguration(AssetContext defaultAssetContext)
        {
            DefaultAssetContext = defaultAssetContext;
            SharedResources = new ResourceRegistry();
            Localizer = new DictionaryLocalizer();
            AssetResolver = new AssetResolver();
            AssetContextFactory = new AssetContextFactory();
        }

        /// <summary>
        /// Gets or sets the default context to use with assets loading.
        /// </summary>
        public IAssetContext DefaultAssetContext { get; set; }

        /// <summary>
        /// Gets or sets an instance of the asset resolver used in the library.
        /// </summary>
        public IAssetResolver AssetResolver { get; set; }

        /// <summary>
        /// Gets or sets where localized strings come from.
        /// </summary>
        /// <remarks>
        /// Defaults to an empty <see cref="DictionaryLocalizer"/>: every key shows as is until the game sets one (see
        /// <see cref="BuildingExtensions.WithLocalizer(IAssetConfigurationBuilder, ILocalizer)"/>).
        /// </remarks>
        public ILocalizer Localizer { get; set; }

        /// <summary>
        /// Gets or sets resources registry used in the library.
        /// </summary>
        public IResourceRegistry SharedResources { get; set; }

        /// <summary>
        /// Gets or sets an instance of the asset context factory to use with current configuration.
        /// </summary>
        public IAssetContextFactory AssetContextFactory { get; set; }

        /// <summary>
        /// Loads an asset from a path relative to <see cref="DefaultAssetContext"/> (the application folder unless
        /// changed), through the importers registered on <see cref="AssetResolver"/>.
        /// </summary>
        /// <typeparam name="T">The asset type, such as <see cref="UI.UIElement"/> for a markup file.</typeparam>
        /// <param name="path">The asset's path, for example <c>"UI/Menu.xml"</c>.</param>
        /// <returns>The loaded asset.</returns>
        /// <exception cref="FileNotFoundException">The file doesn't exist.</exception>
        /// <exception cref="InvalidOperationException">No importer handles the file's format.</exception>
        public T Load<T>(string path)
            where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            return AssetResolver.LoadAsset<T>(DefaultAssetContext, path);
        }

        /// <summary>
        /// Loads an asset asynchronously from a path relative to <see cref="DefaultAssetContext"/>.
        /// </summary>
        /// <typeparam name="T">The asset type.</typeparam>
        /// <param name="path">The asset's path.</param>
        /// <returns>The loaded asset.</returns>
        /// <exception cref="FileNotFoundException">The file doesn't exist.</exception>
        /// <exception cref="InvalidOperationException">No importer handles the file's format.</exception>
        public ValueTask<T> LoadAsync<T>(string path)
            where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            return AssetResolver.LoadAssetAsync<T>(DefaultAssetContext, path);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            AssetResolver.Dispose();
            foreach (var resource in SharedResources.Values)
            {
                if (resource is IDisposable disposable)
                    disposable.Dispose();
            }

            SharedResources.Clear();
            GC.SuppressFinalize(this);
        }
    }
}