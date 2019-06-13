using System;
using System.Diagnostics;
using System.IO;

using Microsoft.Build.Framework;

using THNETII.AzureDevOps.Pipelines.MSBuild.Internal;

using MSBuildLogger = Microsoft.Build.Utilities.Logger;

namespace THNETII.AzureDevOps.Pipelines.MSBuild
{
    public class AzureDevOpsLogger : MSBuildLogger, IForwardingLogger
    {
        private readonly bool isSystemDebugEnv = IsEnvSystemDebug();
        private readonly TextWriter writer;

        [DebuggerStepThrough]
        public AzureDevOpsLogger() : this(Console.Out) { }

        public AzureDevOpsLogger(TextWriter writer) : base()
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        /// <inheritdoc cref="IForwardingLogger.BuildEventRedirector"/>
        public IEventRedirector BuildEventRedirector =>
            ((IForwardingLogger)this).BuildEventRedirector;

        /// <inheritdoc cref="IForwardingLogger.BuildEventRedirector"/>
        IEventRedirector IForwardingLogger.BuildEventRedirector { get; set; }

        /// <inheritdoc cref="IForwardingLogger.NodeId"/>
        public int NodeId => ((IForwardingLogger)this).NodeId;

        /// <inheritdoc cref="IForwardingLogger.NodeId"/>
        int IForwardingLogger.NodeId { get; set; }

        /// <inheritdoc cref="ILogger.Initialize"/>
        public override void Initialize(IEventSource eventSource) =>
            Initialize(eventSource, BuildEventContext.InvalidNodeId);

        /// <inheritdoc cref="INodeLogger.Initialize"/>
        public void Initialize(IEventSource eventSource, int nodeId)
        {
            ((IForwardingLogger)this).NodeId = nodeId;
            if (eventSource is null)
                return;

            eventSource.MessageRaised += OnMessageRaised;
            eventSource.WarningRaised += OnWarningRaised;
            eventSource.ErrorRaised += OnErrorRaised;

            eventSource.AnyEventRaised += (sender, e) => BuildEventRedirector?.ForwardEvent(e);
        }

        private static bool IsEnvSystemDebug()
        {
            string varSystemDebugEnv = Environment.GetEnvironmentVariable("SYSTEM_DEBUG");
            bool isSystemDebugEnv;
            if (string.IsNullOrWhiteSpace(varSystemDebugEnv))
                isSystemDebugEnv = false;
            else
            {
                switch (BooleanStringConverter.ParseOrNull(varSystemDebugEnv))
                {
                    case false:
                        isSystemDebugEnv = false;
                        break;
                    case null:
                    default:
                        isSystemDebugEnv = true;
                        break;
                }
            }

            return isSystemDebugEnv;
        }

        protected virtual void OnMessageRaised(object sender, BuildMessageEventArgs e)
        {
            switch (e?.Importance)
            {
                case MessageImportance.Low when !IsVerbosityAtLeast(LoggerVerbosity.Detailed):
                case MessageImportance.Normal when !IsVerbosityAtLeast(LoggerVerbosity.Normal):
                case MessageImportance.High when !IsVerbosityAtLeast(LoggerVerbosity.Minimal):
                    return;
            }
            if (isSystemDebugEnv && (e?.Message).TryNotNull(out string msg))
                writer.WriteLine(VstsLoggingCommand.FormatTaskDebug(msg));
        }

        protected virtual void OnWarningRaised(object sender, BuildWarningEventArgs e)
        {
            if (e is null)
                return;
            writer.WriteLine(FormatWarningEvent(e));
        }

        protected virtual void OnErrorRaised(object sender, BuildErrorEventArgs e)
        {
            if (e is null)
                return;
            writer.WriteLine(FormatErrorEvent(e));
        }

        private static int? GetVstsTaskOrderOrNull(BuildEventContext context)
        {
            int nodeId = context.NodeId;
            return nodeId == BuildEventContext.InvalidNodeId ? null : (int?)nodeId;
        }

        private static (int lineNumber, int columnNumber) GetLineAndColumnNumber(int lineNumber, int columnNumber, int endColumnNumber)
        {
            if (lineNumber == endColumnNumber && columnNumber == 0 && endColumnNumber == 0)
                return (lineNumber, -1);
            return (lineNumber, columnNumber);
        }

        /// <inheritdoc />
        public override string FormatWarningEvent(BuildWarningEventArgs args)
        {
            var (lineNumber, columnNumber) = GetLineAndColumnNumber(
                args.LineNumber, args.ColumnNumber,
                args.EndColumnNumber
                );
            return VstsLoggingCommand.FormatTaskLogIssue(
                VstsTaskLogIssueType.Warning, args.Message,
                args.Code, args.File, lineNumber, columnNumber
                );
        }

        /// <inheritdoc />
        public override string FormatErrorEvent(BuildErrorEventArgs args)
        {
            var (lineNumber, columnNumber) = GetLineAndColumnNumber(
                args.LineNumber, args.ColumnNumber,
                args.EndColumnNumber
                );
            return VstsLoggingCommand.FormatTaskLogIssue(
                VstsTaskLogIssueType.Error, args.Message,
                args.Code, args.File, lineNumber, columnNumber
                );
        }
    }
}
