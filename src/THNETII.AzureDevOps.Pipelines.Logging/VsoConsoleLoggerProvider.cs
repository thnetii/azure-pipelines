using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    [ProviderAlias("VsoConsole")]
    public sealed class VsoConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private static readonly Dictionary<string, VsoConsoleLogger> loggers =
            new Dictionary<string, VsoConsoleLogger>(StringComparer.OrdinalIgnoreCase);
        private readonly VsoConsoleProcessor consoleProcessor;

        public VsoConsoleLoggerProvider(VsoConsoleProcessor consoleProcessor) : base()
        {
            this.consoleProcessor = consoleProcessor;
        }

        public ILogger CreateLogger(string categoryName)
        {
            lock (loggers)
            {
                if (loggers.TryGetValue(categoryName, out var logger))
                    return logger;
                else
                    return loggers[categoryName] = new VsoConsoleLogger(categoryName, consoleProcessor);
            }
        }

        public void Dispose()
        {
            consoleProcessor.Dispose();
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            lock (loggers)
            {
                foreach (var logger in loggers.Values)
                    logger.ScopeProvider = scopeProvider;
            }
        }
    }
}
