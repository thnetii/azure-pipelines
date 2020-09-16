using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

using THNETII.AzureDevOps.Pipelines.VstsTaskSdk;
using THNETII.TypeConverter;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    public class VsoConsoleFormatter : ConsoleFormatter
    {
        public const string FormatterName = "vso";

        public VsoConsoleFormatter() : base(FormatterName) { }

        public override void Write<TState>(in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && message == null)
            {
                return;
            }

            LogLevel logLevel = logEntry.LogLevel;

            var stateLookup = logEntry.State switch
            {
                IEnumerable<KeyValuePair<string, string>> kvps => kvps
                    .ToLookup(kvp => kvp.Key, kvp => (object)kvp.Value),
                IEnumerable<KeyValuePair<string, object>> kvps => kvps
                    .ToLookup(kvp => kvp.Key, kvp => kvp.Value),
                _ => Enumerable.Empty<KeyValuePair<string, object>>()
                    .ToLookup(kvp => kvp.Key, kvp => kvp.Value)
            };

            var stringToInt = NumberStringConverter.InvariantCulture.Int32;

            var logIssueType = stateLookup["type"].FirstOrDefault() switch
            {
                VstsTaskLogIssueType t => t,
                string s => EnumStringConverter.ParseOrDefault(s, ToVstsTaskLogIssueType(logLevel)),
                object o => EnumStringConverter.ParseOrDefault(o.ToString(), ToVstsTaskLogIssueType(logLevel)),
                _ => ToVstsTaskLogIssueType(logLevel)
            };
            string? errCode = stateLookup["code"].FirstOrDefault()?.ToString();
            string? sourcePath = stateLookup["sourcepath"].FirstOrDefault()?.ToString();
            int lineNumber = stateLookup["linenumber"].FirstOrDefault() switch
            {
                int v => v,
                string s => stringToInt.ParseOrDefault(s, -1),
                object o => stringToInt.ParseOrDefault(o.ToString(), -1),
                _ => -1
            };
            int columnNumber = stateLookup["columnnumber"].FirstOrDefault() switch
            {
                int v => v,
                string s => stringToInt.ParseOrDefault(s, -1),
                object o => stringToInt.ParseOrDefault(o.ToString(), -1),
                _ => -1
            };

            var vsoOutput = logIssueType == VstsTaskLogIssueType.None
                ? VstsLoggingCommand.FormatTaskDebug(message)
                : VstsLoggingCommand.FormatTaskLogIssue(logIssueType, message,
                    errCode, sourcePath, lineNumber, columnNumber);
            if (!string.IsNullOrEmpty(vsoOutput))
                textWriter?.WriteLine(vsoOutput);
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
