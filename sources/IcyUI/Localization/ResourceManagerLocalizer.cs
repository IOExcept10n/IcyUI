// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Globalization;
using System.Resources;

namespace Icy.Localization
{
    /// <summary>
    /// Localized strings from .NET resources (<c>.resx</c> files compiled into the game and its satellite assemblies).
    /// </summary>
    /// <param name="resources">The resources, such as a generated <c>Strings.ResourceManager</c>.</param>
    /// <remarks>
    /// <see cref="ResourceManager"/> already falls back from <c>ru-RU</c> to <c>ru</c> to the neutral resources. The
    /// strings ship inside the build; use <c>PoLocalizer</c> for files players or modders can edit.
    /// </remarks>
    public sealed class ResourceManagerLocalizer(ResourceManager resources) : ILocalizer
    {
        private readonly ResourceManager resources = resources ?? throw new ArgumentNullException(nameof(resources));

        /// <inheritdoc/>
        public string? Localize(string key, CultureInfo culture) => resources.GetString(key, culture);
    }
}
