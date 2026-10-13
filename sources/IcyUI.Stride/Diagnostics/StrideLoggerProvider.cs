// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using Microsoft.Extensions.Logging;
using StrideLogger = Stride.Core.Diagnostics.Logger;

namespace Icy.Stride.Diagnostics
{
    /// <summary>
    /// Forwards IcyUI's log messages to Stride's <c>GlobalLogger</c>, so they appear with Stride's own in the game
    /// console and Game Studio's output. IcyUI.Stride's default log provider.
    /// </summary>
    internal sealed class StrideLoggerProvider : ILoggerProvider
    {
        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) =>
            new Logger(global::Stride.Core.Diagnostics.GlobalLogger.GetLogger(categoryName));

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Writes one category's messages to its Stride logger.
        /// </summary>
        /// <param name="logger">The Stride logger of the category.</param>
        private sealed class Logger(StrideLogger logger) : ILogger
        {
            /// <inheritdoc/>
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            /// <inheritdoc/>
            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            /// <inheritdoc/>
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                string message = formatter(state, exception);
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        logger.Verbose(message, exception);
                        break;
                    case LogLevel.Debug:
                        logger.Debug(message, exception);
                        break;
                    case LogLevel.Information:
                        logger.Info(message, exception);
                        break;
                    case LogLevel.Warning:
                        logger.Warning(message, exception);
                        break;
                    case LogLevel.Error:
                        logger.Error(message, exception);
                        break;
                    case LogLevel.Critical:
                        logger.Fatal(message, exception);
                        break;
                }
            }
        }
    }
}
