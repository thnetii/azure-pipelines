using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    [ProviderAlias("VsoConsole")]
    public sealed class VsoConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private static readonly VsoConsoleProcessor messageQueue =
            new VsoConsoleProcessor();
        private static readonly ConcurrentDictionary<string, VsoConsoleLogger> loggers =
            new ConcurrentDictionary<string, VsoConsoleLogger>(StringComparer.OrdinalIgnoreCase);

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, loggerName => new VsoConsoleLogger(loggerName, messageQueue)
            {
            });
        }

        public void Dispose()
        {
            messageQueue.Dispose();
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            throw new NotImplementedException();
        }
    }
}
