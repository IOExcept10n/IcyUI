// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace Icy.Configuration
{
    /// <summary>
    /// The fonts section of an <see cref="IConfigurationBuilder"/>. The font system exists only once the configuration
    /// is built, so every step in this section is deferred with <see cref="IConfigurationBuilder.AfterBuild"/> and
    /// runs in declaration order. See <see cref="FontBuildingExtensions"/> for the steps.
    /// </summary>
    public interface IFontConfigurationBuilder : IConfigurationBuilder
    {
    }
}
