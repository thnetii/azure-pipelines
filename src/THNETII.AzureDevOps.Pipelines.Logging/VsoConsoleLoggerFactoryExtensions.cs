using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    public static class VsoConsoleLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddVsoConsole(this ILoggingBuilder builder)
        {
            builder?.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, VsoConsoleLoggerProvider>());

            return builder;
        }
    }
}
