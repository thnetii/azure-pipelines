using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

using Xunit;

namespace THNETII.AzureDevOps.Pipelines.Logging.Test
{
    public class VsoConsoleLoggingTest
    {
        [Fact]
        public void Writes_nothing_if_nothing_logged()
        {
            using var writer = new StringWriter();
            using var processor = new VsoStringBuilderLoggingProcessor(writer);
            using var host = new HostBuilder()
                .ConfigureLogging(logging => logging.AddVsoConsole())
                .ConfigureServices(services => services.AddSingleton<VsoConsoleProcessor>(processor))
                .Build();

            host.Start();
            var logger = host.Services.GetRequiredService<ILogger>();
            host.StopAsync().GetAwaiter().GetResult();

            Assert.Empty(writer.ToString());
        }
    }
}
