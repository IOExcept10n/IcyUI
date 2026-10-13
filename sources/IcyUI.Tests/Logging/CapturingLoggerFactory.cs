using Microsoft.Extensions.Logging;

namespace Icy.Tests.Logging
{
    /// <summary>One message written through a <see cref="CapturingLoggerFactory"/>.</summary>
    public sealed record CapturedLog(string Category, LogLevel Level, EventId EventId, string Message, Exception? Exception);

    /// <summary>An <see cref="ILoggerFactory"/> that records every enabled message, for asserting what IcyUI logs.</summary>
    public sealed class CapturingLoggerFactory(LogLevel minimumLevel = LogLevel.Trace) : ILoggerFactory
    {
        private readonly List<CapturedLog> entries = [];
        private readonly LogLevel minimumLevel = minimumLevel;

        public IReadOnlyList<CapturedLog> Entries => entries;

        public void AddProvider(ILoggerProvider provider) => throw new NotSupportedException();

        public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);

        public void Dispose()
        {
        }

        private sealed class Logger(CapturingLoggerFactory owner, string category) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel >= owner.minimumLevel && logLevel != LogLevel.None;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel))
                    owner.entries.Add(new CapturedLog(category, logLevel, eventId, formatter(state, exception), exception));
            }
        }
    }
}
