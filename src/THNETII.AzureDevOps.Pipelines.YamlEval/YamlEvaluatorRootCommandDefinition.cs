using System.CommandLine;
using System.Reflection;

using THNETII.CommandLine.Hosting;

namespace THNETII.AzureDevOps.Pipelines.YamlEval
{
    public class YamlEvaluatorRootCommandDefinition : ICommandDefinition
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

        public YamlEvaluatorRootCommandDefinition()
        {
            RootCommand = new RootCommand(description)
            {
                Handler = Program.RootHandler
            };
        }

        public RootCommand RootCommand { get; }

        Command ICommandDefinition.RootCommand => RootCommand;
    }
}
