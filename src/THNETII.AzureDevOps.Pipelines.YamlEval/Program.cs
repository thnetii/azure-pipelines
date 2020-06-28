using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

using THNETII.CommandLine.Hosting;

namespace THNETII.AzureDevOps.Pipelines.YamlEval
{
    public static class Program
    {
        public static ICommandHandler RootHandler { get; } = CommandHandler.Create(
        (IHost host, CancellationToken cancelToken) =>
        {
            var cmdDefinition = host.Services
                .GetRequiredService<YamlEvaluatorRootCommandDefinition>();
        });

        public static Task<int> Main(string[] args)
        {
            var cmdDefinition = new YamlEvaluatorRootCommandDefinition();

            return DefaultCommandLine.InvokeAsync(cmdDefinition, args,
                ConfigureHost);
        }

        private static void ConfigureHost(IHostBuilder host)
        {
            host.ConfigureServices(services =>
            {
                var innerServices = new ServiceCollection();
                innerServices.AddOptions<InvocationLifetimeOptions>()
                    .Configure(opts => opts.SuppressStatusMessages = true);
                innerServices.AddOptions<ConsoleLoggerOptions>()
                    .Configure(opts => opts.LogToStandardErrorThreshold = LogLevel.Trace);

                foreach (var innerDesc in innerServices.Where(desc => desc
                    .ServiceType.GetGenericTypeDefinition() == typeof(IConfigureOptions<>)))
                {
                    services.Insert(0, innerDesc);
                }
            });
        }
    }
}
