﻿using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using THNETII.AzureDevOps.Pipelines.VstsTaskSdk;
using THNETII.TypeConverter;

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

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            if (formatter is null)
                throw new ArgumentNullException(nameof(formatter));

            var message = formatter(state, exception) ?? exception?.ToString() ??
                string.Empty;

            var stateLookup = state switch
            {
                IEnumerable<KeyValuePair<string, string>> kvps => kvps
                    .ToLookup(kvp => kvp.Key, kvp => (object)(kvp.Value)),
                IEnumerable<KeyValuePair<string, object>> kvps => kvps
                    .ToLookup(kvp => kvp.Key, kvp => kvp.Value),
                _ => Enumerable.Empty<KeyValuePair<string, object>>()
                    .ToLookup(kvp => kvp.Key, kvp => kvp.Value)
            };

            var logIssueType = stateLookup["type"].FirstOrDefault() switch
            {
                VstsTaskLogIssueType t => t,
                string s => EnumStringConverter.ParseOrDefault(s, ToVstsTaskLogIssueType(logLevel)),
                object o => EnumStringConverter.ParseOrDefault(o.ToString(), ToVstsTaskLogIssueType(logLevel)),
                _ => ToVstsTaskLogIssueType(logLevel)
            };
            string errCode = stateLookup["code"].FirstOrDefault()?.ToString();
            string sourcePath = stateLookup["sourcepath"].FirstOrDefault()?.ToString();
            int lineNumber = stateLookup["linenumber"].FirstOrDefault() switch
            {
                int v => v,
                string s => int.TryParse(s, out int v) ? v : -1,
                object o => int.TryParse(o.ToString(), out int v) ? v : -1,
                _ => -1
            };
            int columnNumber = stateLookup["columnnumber"].FirstOrDefault() switch
            {
                int v => v,
                string s => int.TryParse(s, out int v) ? v : -1,
                object o => int.TryParse(o.ToString(), out int v) ? v : -1,
                _ => -1
            };

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