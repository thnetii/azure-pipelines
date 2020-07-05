using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;

namespace THNETII.AzureDevOps.Pipelines.Logging.Test
{
    public static class VsoConsoleLoggingTest
    {
        [Fact]
        public static void GetServicesReturnsVsoConsoleLoggerProviderWhenConsoleAdded()
        {
            using var services = new ServiceCollection()
                .AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddVsoConsole();
                })
                .BuildServiceProvider();

            var loggingProviders = services.GetServices<ILoggerProvider>()
                .ToDictionary(i => i.GetType());
            Assert.Contains(typeof(VsoConsoleLoggerProvider), loggingProviders.Keys);
            Assert.NotNull(loggingProviders[typeof(VsoConsoleLoggerProvider)]);
        }

        [Fact]
        public static void GetServicesReturnsVsoConsoleLoggerProviderWhenConsoleNotAdded()
        {
            using var services = new ServiceCollection()
                .AddLogging(logging =>
                {
                    logging.AddVsoConsole();
                })
                .BuildServiceProvider();

            var loggingProviders = services.GetServices<ILoggerProvider>()
                .ToDictionary(i => i.GetType());
            Assert.Contains(typeof(VsoConsoleLoggerProvider), loggingProviders.Keys);
            Assert.NotNull(loggingProviders[typeof(VsoConsoleLoggerProvider)]);
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        public static void LogsLogLevelBelowWarning(LogLevel logLevel)
        {
            using var services = new ServiceCollection()
                .AddLogging(logging => logging.AddVsoConsole())
                .BuildServiceProvider();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(VsoConsoleLoggingTest));

            logger.Log(logLevel, $"This is a test using loglevel {{{nameof(LogLevel)}}}", logLevel);
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        public static void LogsLogLevelAboveWarning(LogLevel logLevel)
        {
            using var services = new ServiceCollection()
                .AddLogging(logging => logging.AddVsoConsole())
                .BuildServiceProvider();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(VsoConsoleLoggingTest));

            logger.Log(logLevel, $"This is a test using loglevel {{{nameof(LogLevel)}}}", logLevel);
        }

        [Fact]
        public static void LogsLogLevelNone()
        {
            const LogLevel logLevel = LogLevel.None;
            using var services = new ServiceCollection()
                .AddLogging(logging => logging.AddVsoConsole())
                .BuildServiceProvider();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(VsoConsoleLoggingTest));

            logger.Log(logLevel, $"This is a test using loglevel {{{nameof(LogLevel)}}}", logLevel);
        }

        [Fact]
        public static void LogsWithStringDictionaryState()
        {
            using var services = new ServiceCollection()
                .AddLogging(logging => logging.AddVsoConsole())
                .BuildServiceProvider();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(VsoConsoleLoggingTest));

            var dict = new Dictionary<string, string>
            {
                ["code"] = "TEST"
            };

            logger.Log(LogLevel.Warning, default, dict, null, (state, except) => "Message");
        }

        [Fact]
        public static void LogsWithObjectDictionaryState()
        {
            using var services = new ServiceCollection()
                .AddLogging(logging => logging.AddVsoConsole())
                .BuildServiceProvider();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(typeof(VsoConsoleLoggingTest));

            var dict = new Dictionary<string, object>
            {
                ["code"] = "TEST"
            };

            logger.Log(LogLevel.Warning, default, dict, null, (state, except) => "Message");
        }
    }
}
