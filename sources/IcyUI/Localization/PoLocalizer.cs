// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Globalization;
using Icy.Assets;
using Icy.Configuration;
using Icy.Diagnostics.Logging;
using Microsoft.Extensions.Logging;

namespace Icy.Localization
{
    /// <summary>
    /// Localized strings from gettext <c>.po</c> files, one per language, shipped as plain files next to the game.
    /// </summary>
    /// <param name="assets">
    /// The asset configuration whose <see cref="AssetConfiguration.DefaultAssetContext"/> the files are read from.
    /// </param>
    /// <param name="folder">The folder of the files, relative to the asset context, such as <c>Localization</c>.</param>
    /// <param name="defaultCulture">
    /// The language used when a culture and its parents have no string, or <see langword="null"/> for none: then a
    /// language without a file has no strings here. Leave it out for a layer in a <see cref="CompositeLocalizer"/>, such
    /// as mod files, so its default language doesn't hide the next localizer's strings for the player's language.
    /// </param>
    /// <param name="logger">Where an unreadable file is reported, or <see langword="null"/>.</param>
    /// <remarks>
    /// <para>
    /// For <c>ru-RU</c> it reads <c>ru-RU.po</c> (or gettext's <c>ru_RU.po</c>), then <c>ru.po</c>, then the default
    /// culture's file. Each file is read once, the first time its language is needed; a missing file is remembered as
    /// missing. A file that isn't valid is reported once (event 4002) and treated as missing, so a typo in a translation
    /// never stops the game. Translators edit <c>.po</c> files with tools such as Poedit, Weblate or Crowdin, and players can add a
    /// language without a new build. See <see cref="PoCatalog"/> for what's read.
    /// </para>
    /// <para>Not thread-safe: use it from the thread that runs the UI.</para>
    /// </remarks>
    public sealed class PoLocalizer(AssetConfiguration assets, string folder, CultureInfo? defaultCulture = null, ILogger? logger = null) : ILocalizer
    {
        private readonly AssetConfiguration assets = assets ?? throw new ArgumentNullException(nameof(assets));
        private readonly string folder = folder ?? throw new ArgumentNullException(nameof(folder));
        private readonly Dictionary<string, PoCatalog?> catalogs = new(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public string? Localize(string key, CultureInfo culture)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(culture);
            return Find(key, culture) ?? (defaultCulture == null ? null : Find(key, defaultCulture));
        }

        private string? Find(string key, CultureInfo culture)
        {
            for (CultureInfo current = culture; !current.Equals(CultureInfo.InvariantCulture); current = current.Parent)
            {
                if (Catalog(current.Name) is { } catalog && catalog.TryGetString(key, out string? text))
                    return text;
            }

            return null;
        }

        private PoCatalog? Catalog(string cultureName)
        {
            if (catalogs.TryGetValue(cultureName, out PoCatalog? catalog))
                return catalog;

            IAssetContext context = assets.DefaultAssetContext;
            foreach (string name in (string[])[cultureName, cultureName.Replace('-', '_')])
            {
                string path = folder.Length == 0 ? $"{name}.po" : $"{folder.TrimEnd('/', '\\')}/{name}.po";
                if (!context.IsAvailable(path))
                    continue;

                try
                {
                    using Stream stream = context.OpenStream(path);
                    using var reader = new StreamReader(stream);
                    catalog = PoCatalog.Parse(reader, path);
                }
                catch (Exception ex) when (ex is FormatException or IOException)
                {
                    if (logger != null)
                        IcyLog.LocalizationFileUnreadable(logger, path, ex);
                }

                break;
            }

            catalogs[cultureName] = catalog;
            return catalog;
        }
    }
}
