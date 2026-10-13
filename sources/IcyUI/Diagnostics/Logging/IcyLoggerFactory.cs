// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.Logging;

namespace Icy.Diagnostics.Logging
{
    /// <summary>
    /// IcyUI's own minimal <see cref="ILoggerFactory"/>: writes to a fixed set of providers at or above a minimum level.
    /// Exists so core needs only the logging abstractions, not <c>Microsoft.Extensions.Logging</c> itself.
    /// </summary>
    internal sealed class IcyLoggerFactory(LogLevel minimumLevel, IEnumerable<ILoggerProvider> providers) : ILoggerFactory
    {
        private readonly ILoggerProvider[] providers = [.. providers];

        /// <inheritdoc/>
        public void AddProvider(ILoggerProvider provider) =>
            throw new NotSupportedException("Add providers through the configuration builder's logging section.");

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) =>
            new Logger(minimumLevel, [.. providers.Select(p => p.CreateLogger(categoryName))]);

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        private sealed class Logger(LogLevel minimumLevel, ILogger[] loggers) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel)
            {
                if (logLevel == LogLevel.None || logLevel < minimumLevel)
                    return false;
                foreach (ILogger logger in loggers)
                {
                    if (logger.IsEnabled(logLevel))
                        return true;
                }

                return false;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.None || logLevel < minimumLevel)
                    return;
                foreach (ILogger logger in loggers)
                {
                    if (logger.IsEnabled(logLevel))
                        logger.Log(logLevel, eventId, state, exception, formatter);
                }
            }
        }
    }
}
