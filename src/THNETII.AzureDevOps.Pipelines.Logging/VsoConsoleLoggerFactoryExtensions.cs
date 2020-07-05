using System;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    public static class VsoConsoleLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddVsoConsole(this ILoggingBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            if (builder.Services is IServiceCollection services)
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, VsoConsoleLoggerProvider>());
            }

            return builder;
        }
    }
}
