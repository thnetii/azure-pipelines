using System;
using System.IO;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    public class VsoStringBuilderLoggingProcessor : VsoConsoleProcessor
    {
        public VsoStringBuilderLoggingProcessor(TextWriter writer) : base()
        {
            Writer = writer;
        }

        public TextWriter Writer { get; }

        protected override void WriteMessage(string message) =>
            (Writer ?? Console.Out).WriteLine(message);
    }
}
