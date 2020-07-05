using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using System;
using System.Collections.Generic;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    [ProviderAlias("VsoConsole")]
    public sealed class VsoConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private static readonly Dictionary<string, VsoConsoleLogger> loggers =
            new Dictionary<string, VsoConsoleLogger>(StringComparer.OrdinalIgnoreCase);
        private readonly ConsoleLoggerProvider parentProvider;

        public VsoConsoleLoggerProvider(ConsoleLoggerProvider parentProvider) : base()
        {
            this.parentProvider = parentProvider;
        }

        public ILogger CreateLogger(string categoryName)
        {
            VsoConsoleLogger? logger;
            lock (loggers)
            {
                if (loggers.TryGetValue(categoryName, out logger))
                    return logger;
            }
            var conLogger = parentProvider.CreateLogger(categoryName);
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
            parentProvider.SetScopeProvider(scopeProvider);
        }
    }
}
