using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using System;
using System.Collections.Generic;
using System.Linq;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    [ProviderAlias("VsoConsole")]
    public sealed class VsoConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private static readonly Dictionary<string, VsoConsoleLogger> loggers =
            new Dictionary<string, VsoConsoleLogger>(StringComparer.OrdinalIgnoreCase);
        private readonly IServiceProvider serviceProvider;
        private readonly Lazy<ConsoleLoggerProvider> parentProvider;

        private ConsoleLoggerProvider ParentProvider =>
            parentProvider.Value;

        public VsoConsoleLoggerProvider(IServiceProvider serviceProvider) : base()
        {
            this.serviceProvider = serviceProvider;
            parentProvider = new Lazy<ConsoleLoggerProvider>(InitializeParentProvider);
        }

        private ConsoleLoggerProvider InitializeParentProvider()
        {
            return (serviceProvider?.GetServices<ILoggerProvider>()?
                .FirstOrDefault(p => p is ConsoleLoggerProvider)
                as ConsoleLoggerProvider) ?? ActivatorUtilities
                .CreateInstance<ConsoleLoggerProvider>(serviceProvider);
        }

        public ILogger CreateLogger(string categoryName)
        {
            VsoConsoleLogger? logger;
            lock (loggers)
            {
                if (loggers.TryGetValue(categoryName, out logger))
                    return logger;
            }
            var conLogger = ParentProvider.CreateLogger(categoryName);
            logger = new VsoConsoleLogger(categoryName, conLogger);
            lock (loggers)
            {
                if (loggers.TryGetValue(categoryName, out var existingLogger))
                    return existingLogger;
                return loggers[categoryName] = logger;
            }
        }

        public void Dispose()
        {
            lock (loggers) { loggers.Clear(); }
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            ParentProvider.SetScopeProvider(scopeProvider);
        }
    }
}
