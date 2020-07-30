using System;
using System.CommandLine.Parsing;
using System.IO;
using System.Text;

using Microsoft.Extensions.Logging;

using YamlDotNet.RepresentationModel;

namespace THNETII.AzureDevOps.Pipelines.YamlEval
{
    public class YamlEvaluatorCommandLineContext
    {
        private readonly YamlEvaluatorCommandDefinition definition;
        private readonly ILogger logger;
        private readonly FileInfo? inputFileInfo;
        private readonly string? outputPath;
        private readonly Encoding? inputEncoding;
        private readonly Encoding? outputEncoding;

        public YamlEvaluatorCommandLineContext(
            YamlEvaluatorCommandDefinition definition,
            ParseResult cmdParseResult,
            ILoggerFactory? loggerFactory = null)
        {
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _ = cmdParseResult ?? throw new ArgumentNullException(nameof(cmdParseResult));
            loggerFactory ??= Microsoft.Extensions.Logging.Abstractions
                .NullLoggerFactory.Instance;

            logger = loggerFactory.CreateLogger(typeof(Program).Namespace);

            if (cmdParseResult.FindResultFor(definition.InputArgument) is { } inputArgument)
                inputFileInfo = inputArgument.GetValueOrDefault<FileInfo>();

            if (cmdParseResult.FindResultFor(definition.OutputOption) is { } outputOption)
                outputPath = outputOption.GetValueOrDefault<string>();

            if (cmdParseResult.FindResultFor(definition.InputEncodingOption) is { } inputEncodingOption)
                inputEncoding = inputEncodingOption.GetValueOrDefault<Encoding>();

            if (cmdParseResult.FindResultFor(definition.OutputEncodingOption) is { } outputEncodingOption)
                outputEncoding = outputEncodingOption.GetValueOrDefault<Encoding>();
        }

        public YamlStream CreateYamlStream()
        {
            YamlStream yaml = new YamlStream();
            using (TextReader reader = inputFileInfo is null
                ? GetConsoleTextReader(logger, definition.InputArgument.Name, inputEncoding)
                : inputEncoding is null
                ? inputFileInfo.OpenText()
                : new StreamReader(inputFileInfo.OpenRead(), inputEncoding))
            {
                yaml.Load(reader);
            }
            return yaml;

            static TextReader GetConsoleTextReader(ILogger logger, string inputName, Encoding? encoding)
            {
                logger.LogTrace(new EventId(0, inputName),
                    "No input file specified by command-line arguments. Using Standard Input Stream as input.");
                if (encoding is null)
                    return Console.In;
                return new StreamReader(Console.OpenStandardInput(), encoding, leaveOpen: true);
            }
        }

        public void WriteYamlStream(YamlStream yaml)
        {
            yaml ??= new YamlStream();

            using (TextWriter writer = string.IsNullOrWhiteSpace(outputPath)
                ? GetConsoleTextWriter(logger, definition.OutputOption.Name, outputEncoding)
                : new StreamWriter(outputPath, append: false, outputEncoding ?? Encoding.UTF8) { AutoFlush = true })
                yaml.Save(writer);

            static TextWriter GetConsoleTextWriter(ILogger logger, string outputName, Encoding? encoding)
            {
                logger.LogTrace(new EventId(1, outputName),
                    "No output file specified by command-line arguments. Using Standard Output Stream as output.");
                if (encoding is null)
                    return Console.Out;
                return new StreamWriter(Console.OpenStandardOutput(), encoding, leaveOpen: true)
                { AutoFlush = true };
            }
        }
    }
}
