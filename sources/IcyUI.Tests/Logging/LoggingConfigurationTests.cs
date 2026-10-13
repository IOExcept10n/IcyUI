using System.Diagnostics;
using Icy.Configuration;
using Icy.Diagnostics.Logging;
using Icy.Tests.Input;
using Icy.Tests.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Icy.Tests.Logging
{
    public class LoggingConfigurationTests
    {
        private static IConfigurationBuilder NewBuilder() =>
            new IcyConfigurationBuilder().ConfigureRendering(new FakeRenderContext()).ConfigureInput(new FakeInputSystem());

        [Fact]
        public void Logging_DefaultsToNullFactory()
        {
            IcyConfiguration configuration = NewBuilder().Build();

            Assert.Same(NullLoggerFactory.Instance, configuration.Logging);
        }

        [Fact]
        public void UseLoggerFactory_IsUsedAsIs()
        {
            var factory = new CapturingLoggerFactory();

            IcyConfiguration configuration = NewBuilder().ConfigureLogging(factory).Build();

            Assert.Same(factory, configuration.Logging);
        }

        [Fact]
        public void Providers_ReceiveMessagesAtOrAboveTheMinimumLevel()
        {
            var sink = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder()
                .ConfigureLogging()
                .AddLoggerProvider(new FactoryProvider(sink))
                .Build();
            ILogger logger = configuration.Logging.CreateLogger("Icy.Test");

            logger.LogInformation("hidden");
            logger.LogWarning("shown");

            CapturedLog entry = Assert.Single(sink.Entries);
            Assert.Equal("shown", entry.Message);
            Assert.Equal("Icy.Test", entry.Category);
        }

        [Fact]
        public void ConfigureLogging_Level_LowersTheMinimum()
        {
            var sink = new CapturingLoggerFactory();
            IcyConfiguration configuration = NewBuilder()
                .ConfigureLogging(LogLevel.Debug)
                .AddLoggerProvider(new FactoryProvider(sink))
                .Build();

            configuration.Logging.CreateLogger("Icy.Test").LogDebug("detail");

            Assert.Single(sink.Entries);
        }

        [Fact]
        public void ConfigureLogging_Action_ReachesTheSection()
        {
            IConfigurationBuilder builder = NewBuilder();

            builder.ConfigureLogging(l => l.MinimumLevel = LogLevel.Error);

            Assert.Equal(LogLevel.Error, builder.ConfigureLogging().Logging.MinimumLevel);
        }

        [Fact]
        public void DebuggerLoggerProvider_IsEnabledOnlyWhileADebuggerLogs()
        {
            using var provider = new DebuggerLoggerProvider();

            Assert.Equal(Debugger.IsLogging(), provider.CreateLogger("Icy.Test").IsEnabled(LogLevel.Warning));
            Assert.False(provider.CreateLogger("Icy.Test").IsEnabled(LogLevel.None));
        }

        private sealed class FactoryProvider(ILoggerFactory sink) : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName) => sink.CreateLogger(categoryName);

            public void Dispose()
            {
            }
        }
    }
}
