using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace THNETII.AzureDevOps.Pipelines.YamlEval
{
    public class YamlEvaluatorCommandDefinition
    {
        private static readonly Assembly programAssembly =
            typeof(Program).Assembly;
        private static readonly string description = (programAssembly
            .GetCustomAttribute<AssemblyDescriptionAttribute>()?
            .Description is string descr &&
            !string.IsNullOrWhiteSpace(descr)
            ? descr : programAssembly
            .GetCustomAttribute<AssemblyProductAttribute>()?
            .Product is string product &&
            !string.IsNullOrWhiteSpace(product)
            ? product : null) ?? string.Empty;
        
        public YamlEvaluatorCommandDefinition()
        {
            RootCommand = new RootCommand(description)
            {
                Handler = Program.RootHandler
            };

            InputArgument = new Argument<FileInfo>("INPUT")
            {
                Description = "The YAML template to process",
                Arity = ArgumentArity.ZeroOrOne,
            };
            InputArgument.ExistingOnly();
            RootCommand.AddArgument(InputArgument);

            OutputOption = new Option<string>("--output")
            {
                Description = "Output file path",
                Argument =
                {
                    Name = "OUTPUT",
                    Description = "File path to output YAML file",
                    Arity = ArgumentArity.ZeroOrOne,
                }
            };
            OutputOption.AddAlias("-o");
            OutputOption.LegalFilePathsOnly();
            RootCommand.AddOption(OutputOption);

            //ParameterScalarOption = new Option<string[]>("--parameter")
            //{
            //    Description = "Template parameter scalar value",
            //    Argument =
            //    {
            //        Name = "KEY=VALUE",
            //        Description = "Key-Value-Pair specifying a scalar parameter value",
            //        Arity = ArgumentArity.ZeroOrMore,
            //    },
            //};
            //ParameterScalarOption.AddAlias("-p");
            //RootCommand.AddOption(ParameterScalarOption);

            //ParameterFileOption = new Option<FileInfo[]>("--parameter-file")
            //{
            //    Description = "Template parameter from file",
            //    Argument =
            //    {
            //        Name = "YAML",
            //        Description = "File path to YAML file containing parameters",
            //        Arity = ArgumentArity.OneOrMore
            //    },
            //}.ExistingOnly();
            //RootCommand.AddOption(ParameterFileOption);

            InputEncodingOption = new Option<Encoding>("--input-encoding",
                ParseEncodingArgument)
            {
                Description = "Text encoding for input YAML template",
                Argument =
                {
                    Name = "CHARSET",
                    Description = "IANA charset name for input encoding",
                    Arity = ArgumentArity.ZeroOrOne,
                },
            };
            InputEncodingOption.Argument.AddValidator(ValidateEncodingArgument);
            RootCommand.AddOption(InputEncodingOption);

            OutputEncodingOption = new Option<Encoding>("--output-encoding")
            {
                Description = "Text encoding for output YAML template",
                Argument =
                {
                    Name = "CHARSET",
                    Description = "IANA charset name for output encoding",
                    Arity = ArgumentArity.ZeroOrOne,
                },
            };
            OutputEncodingOption.Argument.AddValidator(ValidateEncodingArgument);
            RootCommand.AddOption(OutputEncodingOption);
        }

        public RootCommand RootCommand { get; }
        public Argument<FileInfo> InputArgument { get; }
        public Option<string> OutputOption { get; }
        //public Option<string[]> ParameterScalarOption { get; }
        //public Option<FileInfo[]> ParameterFileOption { get; }
        public Option<Encoding> InputEncodingOption { get; }
        public Option<Encoding> OutputEncodingOption { get; }

        public static ParseArgument<Encoding> ParseEncodingArgument { get; } =
            argResult => Encoding.GetEncoding(argResult.Tokens.Single().Value);

        public static ValidateSymbol<ArgumentResult> ValidateEncodingArgument { get; } =
        (ArgumentResult argResult) => argResult.Tokens.Select(t => t.Value)
            .Select(charset =>
            {
                try
                {
                    _ = Encoding.GetEncoding(charset);
                    return null;
                }
                catch (ArgumentException argExcept)
                { return argExcept; }
            })
            .Where(except => !(except is null))
            .Select(except => except!.Message)
            .FirstOrDefault();
    }
}
