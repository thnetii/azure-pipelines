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
        private readonly VsoConsoleProcessor loggerProcessor;
        private IExternalScopeProvider scopeProvider;

        public VsoConsoleLoggerProvider(VsoConsoleProcessor loggerProcessor) : base()
        {
            this.loggerProcessor = loggerProcessor;
        }

        public ILogger CreateLogger(string categoryName)
        {
            VsoConsoleLogger logger;
            lock (loggers)
            {
                if (loggers.TryGetValue(categoryName, out logger))
                    return logger;
            }
            logger = new VsoConsoleLogger(categoryName, loggerProcessor)
            { ScopeProvider = scopeProvider };
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
            this.scopeProvider = scopeProvider;
            lock (loggers)
            {
                foreach (var logger in loggers.Values)
                    logger.ScopeProvider = scopeProvider;
            }
        }
    }
}
