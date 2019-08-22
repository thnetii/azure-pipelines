using System;
using Microsoft.Extensions.Logging;
using THNETII.AzureDevOps.Pipelines.VstsTaskSdk;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    internal class VsoConsoleLogger : ILogger
    {
        public string Name { get; }
        internal IExternalScopeProvider ScopeProvider { get; set; }
        internal VsoConsoleProcessor LoggerProcessor { get; }

        public VsoConsoleLogger(string name, VsoConsoleProcessor loggerProcessor)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            LoggerProcessor = loggerProcessor;
        }

        public IDisposable BeginScope<TState>(TState state) =>
            ScopeProvider?.Push(state) ?? VsoNullLoggingScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (formatter is null)
                throw new ArgumentNullException(nameof(formatter));

            var text = formatter(state, exception) ?? exception?.ToString() ??
                string.Empty;


        }
    }
}
