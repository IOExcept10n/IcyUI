// Copyright (c) IOExcept10n (https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Icy.Diagnostics.Logging
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> that writes to an attached debugger's output window, such as Visual Studio's
    /// Output pane. IcyUI.MonoGame uses it by default.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Messages are written with <see cref="Debugger.Log(int, string, string)"/> only while
    /// <see cref="Debugger.IsLogging"/> returns <see langword="true"/>. Without a debugger every logger reports itself
    /// disabled, so logging costs one check.
    /// </para>
    /// <para>
    /// Unlike <c>Debug.WriteLine</c>, which the compiler removes from Release builds of the calling assembly, this
    /// works from IcyUI's Release package.
    /// </para>
    /// </remarks>
    public sealed class DebuggerLoggerProvider : ILoggerProvider
    {
        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName) => new DebuggerLogger(categoryName);

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        private sealed class DebuggerLogger(string category) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && Debugger.IsLogging();

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;
                string message = $"{logLevel}: {category}: {formatter(state, exception)}";
                if (exception != null)
                    message += Environment.NewLine + exception;
                Debugger.Log((int)logLevel, category, message + Environment.NewLine);
            }
        }
    }
}
