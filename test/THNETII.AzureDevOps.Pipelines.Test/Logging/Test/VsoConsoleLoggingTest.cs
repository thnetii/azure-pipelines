using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;

namespace THNETII.AzureDevOps.Pipelines.Logging.Test
{
    public static class VsoConsoleLoggingTest
    {
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
