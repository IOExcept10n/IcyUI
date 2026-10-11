// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.Logging;

namespace Icy.Configuration
{
    /// <summary>
    /// Describes where IcyUI's log messages go: either a whole <see cref="ILoggerFactory"/> supplied by the game, or
    /// IcyUI's own minimal factory feeding the <see cref="Providers"/> at or above <see cref="MinimumLevel"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The engine integrations add a default provider (Stride's log on Stride, the debugger's output window on
    /// MonoGame). Set <see cref="Factory"/> to send everything to your own logging setup instead, for example a
    /// factory created with <c>LoggerFactory.Create(b =&gt; b.AddConsole())</c>.
    /// </para>
    /// <para>
    /// IcyUI never disposes a <see cref="Factory"/> it didn't create.
    /// </para>
    /// </remarks>
    public sealed class LoggingConfiguration
    {
        /// <summary>
        /// Gets or sets a logger factory that replaces IcyUI's own. When it's set, <see cref="MinimumLevel"/> and
        /// <see cref="Providers"/> are ignored and filtering is the factory's job.
        /// </summary>
        public ILoggerFactory? Factory { get; set; }

        /// <summary>
        /// Gets or sets the lowest level IcyUI's own factory passes to <see cref="Providers"/>. Defaults to
        /// <see cref="LogLevel.Warning"/>.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Warning;

        /// <summary>
        /// Gets the providers IcyUI's own factory writes to. Empty means nothing is logged.
        /// </summary>
        public IList<ILoggerProvider> Providers { get; } = [];
    }
}
