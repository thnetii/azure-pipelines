using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;

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
            if (logLevel == LogLevel.None)
                return;
            if (formatter is null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception) ?? exception?.ToString() ??
                string.Empty;

            var logIssueType = ToVstsTaskLogIssueType(logLevel);

            string errCode = null;
            string sourcePath = null;
            int lineNumber = -1;
            int columnNumber = -1;

            var vsoOutput = logIssueType == VstsTaskLogIssueType.None
                ? VstsLoggingCommand.FormatTaskDebug(message)
                : VstsLoggingCommand.FormatTaskLogIssue(logIssueType, message,
                    errCode, sourcePath, lineNumber, columnNumber);
            if (!string.IsNullOrEmpty(vsoOutput))
                LoggerProcessor.EnqueueMessage(vsoOutput);
        }

        private static VstsTaskLogIssueType ToVstsTaskLogIssueType(LogLevel logLevel)
        {
            VstsTaskLogIssueType logIssueType;
            switch (logLevel)
            {
                default:
                    logIssueType = VstsTaskLogIssueType.None;
                    break;
                case LogLevel.Warning:
                    logIssueType = VstsTaskLogIssueType.Warning;
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    logIssueType = VstsTaskLogIssueType.Error;
                    break;
            }

            return logIssueType;
        }
    }
}
