using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    public static class VsoConsoleExtensions
    {
        public static ILoggingBuilder AddVsoConsole(this ILoggingBuilder builder)
        {
            return builder.AddConsole()
                .AddConsoleFormatter<VsoConsoleFormatter, ConsoleFormatterOptions>();
        }
    }
}
