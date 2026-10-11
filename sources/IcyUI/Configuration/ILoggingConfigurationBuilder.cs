// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
namespace Icy.Configuration
{
    /// <summary>
    /// The logging section of an <see cref="IConfigurationBuilder"/>.
    /// </summary>
    public interface ILoggingConfigurationBuilder : IConfigurationBuilder
    {
        /// <summary>
        /// Gets the logging options the built configuration's <see cref="IcyConfiguration.Logging"/> is created from.
        /// </summary>
        LoggingConfiguration Logging { get; }
    }
}
