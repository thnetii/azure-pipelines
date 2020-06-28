using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

using Xunit;

namespace THNETII.AzureDevOps.Pipelines.YamlEval.Test
{
    public static class ConfigurationBinderTest
    {
        public class BinderTarget
        {
            public string[] StringArray { get; set; } = null!;
            public Dictionary<string, string>[] ComplexArray { get; set; } = null!;
        }

        [Fact]
        public static void ConfigurationBindSupportsStringArray()
        {
            var source = new Dictionary<string, string>
            {
                [ConfigurationPath.Combine(nameof(BinderTarget.StringArray), $"{0}")] = $"Entry{0}",
                [ConfigurationPath.Combine(nameof(BinderTarget.StringArray), $"{1}")] = $"Entry{1}",
                [ConfigurationPath.Combine(nameof(BinderTarget.StringArray), $"{2}")] = $"Entry{2}",
            };
            var configuraion = new ConfigurationBuilder()
                .AddInMemoryCollection(source).Build();

            var binderTarget = new BinderTarget();
            configuraion.Bind(binderTarget);

            Assert.Equal(binderTarget.StringArray, source.Values);
        }

        [Fact]
        public static void ConfigurationBindSupportsComplexArray()
        {
            var source = new Dictionary<string, string>
            {
                [ConfigurationPath.Combine(nameof(BinderTarget.ComplexArray), $"{0}", "Property1")] = $"Entry{0}",
                [ConfigurationPath.Combine(nameof(BinderTarget.ComplexArray), $"{0}", "Property2")] = $"Entry{1}",
                [ConfigurationPath.Combine(nameof(BinderTarget.ComplexArray), $"{1}", "Property1")] = $"Entry{0}",
                [ConfigurationPath.Combine(nameof(BinderTarget.ComplexArray), $"{1}", "Property2")] = $"Entry{1}",
                [ConfigurationPath.Combine(nameof(BinderTarget.ComplexArray), $"{2}", "Property1")] = $"Entry{0}",
                [ConfigurationPath.Combine(nameof(BinderTarget.ComplexArray), $"{2}", "Property2")] = $"Entry{1}",
            };
            var configuraion = new ConfigurationBuilder()
                .AddInMemoryCollection(source).Build();

            var binderTarget = new BinderTarget();
            configuraion.Bind(binderTarget);

            Assert.Equal(3, binderTarget.ComplexArray.Length);
            Assert.DoesNotContain(null!, binderTarget.ComplexArray);
            Assert.All(binderTarget.ComplexArray, dict => Assert.Equal(new Dictionary<string, string>
            {
                ["Property1"] = $"Entry{0}",
                ["Property2"] = $"Entry{1}",
            }, dict));
        }
    }
}
