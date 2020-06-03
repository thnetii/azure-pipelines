using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    public static class VsoConsoleLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddVsoConsole(this ILoggingBuilder builder)
        {
            if (builder?.Services is IServiceCollection services)
            {
                services.AddSingleton<VsoConsoleProcessor>();
                services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, VsoConsoleLoggerProvider>());
            }

            return builder;
        }
    }
}
