using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

using Xunit;

namespace THNETII.AzureDevOps.Pipelines.Logging.Test
{
    public static class VsoConsoleLoggingTest
    {
        private static string GetVsoConsoleOutput(Action<ILogger> loggingAction)
        {
            using var writer = new StringWriter();
            using (var serviceProvider = new ServiceCollection()
                .AddLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddVsoConsole();
                })
                .AddSingleton<VsoConsoleProcessor>(_ => new VsoStringBuilderLoggingProcessor(writer))
                .BuildServiceProvider())
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(typeof(VsoConsoleLoggingTest));

                loggingAction(logger);
            }
            return writer.ToString();
        }

        [Fact]
        public static void Writes_nothing_if_nothing_logged()
        {
            string output = GetVsoConsoleOutput(logger => { });

            Assert.Empty(output);
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        public static void Write_task_debug_message_if_log_level_below_warning(LogLevel logLevel)
        {
            string output = GetVsoConsoleOutput(logger =>
            {
                logger.Log(logLevel, nameof(Write_task_debug_message_if_log_level_below_warning));
            });

            Assert.StartsWith("##vso[task.debug]", output, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        public static void Write_task_log_issue_warning_message_if_log_level_above_warning(LogLevel logLevel)
        {
            string output = GetVsoConsoleOutput(logger =>
            {
                logger.Log(logLevel, nameof(Write_task_log_issue_warning_message_if_log_level_above_warning));
            });

            Assert.StartsWith("##vso[task.logissue type=Error]", output, StringComparison.Ordinal);
        }

        [Fact]
        public static void Writes_nothing_if_log_level_is_none()
        {
            string output = GetVsoConsoleOutput(logger =>
            {
                logger.Log(LogLevel.None, nameof(Write_task_log_issue_warning_message_if_log_level_above_warning));
            });

            Assert.Empty(output);
        }

        [Fact]
        public static void Writes_log_issue_message_with_properties_from_string_dictionary_state()
        {
            string output = GetVsoConsoleOutput(logger =>
            {
                var dict = new Dictionary<string, string>
                {
                    ["code"] = "TEST"
                };

                logger.Log(LogLevel.Warning, default, dict, null, (state, except) => "Message");
            });

            Assert.Equal("##vso[task.logissue type=Warning;code=TEST]Message\n", output,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public static void Writes_log_issue_message_with_properties_from_object_dictionary_state()
        {
            string output = GetVsoConsoleOutput(logger =>
            {
                var dict = new Dictionary<string, object>
                {
                    ["code"] = "TEST"
                };

                logger.Log(LogLevel.Warning, default, dict, null, (state, except) => "Message");
            });

            Assert.Equal("##vso[task.logissue type=Warning;code=TEST]Message\n", output,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public static void Writes_log_issue_message_with_properties_from_formatted_log_values()
        {
            string output = GetVsoConsoleOutput(logger =>
            {
                logger.LogWarning("Message: {code}", "TEST");
            });

            Assert.Equal("##vso[task.logissue type=Warning;code=TEST]Message: TEST\n", output,
                ignoreLineEndingDifferences: true);
        }
    }
}
