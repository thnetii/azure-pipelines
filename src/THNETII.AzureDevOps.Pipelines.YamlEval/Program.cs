using System;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace THNETII.AzureDevOps.Pipelines.YamlEval
{
    public static class Program
    {
        public static ICommandHandler RootHandler { get; } = CommandHandler.Create(
        (IHost host, CancellationToken cancelToken) =>
        {
            var context = ActivatorUtilities
                .CreateInstance<YamlEvaluatorCommandLineContext>(host.Services);
        });

        public static Task<int> Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text
                .CodePagesEncodingProvider.Instance);

            var cmdDefinition = new YamlEvaluatorCommandDefinition();
            var cmdParser = new CommandLineBuilder(cmdDefinition.RootCommand)
                .UseDefaults()
                //.UseHelpBuilder(context => new YamlEvaluatorHelpBuilder(context, cmdDefinition))
                .UseHost(Host.CreateDefaultBuilder,
                    host => ConfigureHost(host, cmdDefinition))
                .Build();
            return cmdParser.InvokeAsync(args ?? Array.Empty<string>());
        }

        private static void ConfigureHost(IHostBuilder host, YamlEvaluatorCommandDefinition cmdDefinition)
        {
            host.ConfigureAppConfiguration((context, config) =>
            {
                var fileProvider = new EmbeddedFileProvider(typeof(Program).Assembly);
                var hostingEnvironment = context.HostingEnvironment;

                var sources = config.Sources;
                int originalSourcesCount = sources.Count;

                config.AddJsonFile(fileProvider,
                    $"appsettings.json",
                    optional: true, reloadOnChange: true);
                config.AddJsonFile(fileProvider,
                    $"appsettings.{hostingEnvironment.EnvironmentName}.json",
                    optional: true, reloadOnChange: true);

                const int insert_idx = 1;
                for (int i_dst = insert_idx, i_src = originalSourcesCount;
                    i_src < sources.Count; i_dst++, i_src++)
                {
                    var configSource = sources[i_src];
                    sources.RemoveAt(i_src);
                    sources.Insert(i_dst, configSource);
                }
            });
            host.ConfigureServices(services =>
            {
                services.AddSingleton(cmdDefinition);
                services.AddOptions<InvocationLifetimeOptions>()
                    .Configure(opts => opts.SuppressStatusMessages = true)
                    .Configure<IConfiguration>((opts, config) =>
                        config.Bind("Lifetime", opts));
                services.AddOptions<ConsoleLoggerOptions>()
                    .Configure(opts => opts.LogToStandardErrorThreshold = LogLevel.Trace);
            });
        }
    }
}
