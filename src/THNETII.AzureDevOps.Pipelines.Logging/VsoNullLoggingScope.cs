using System;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    internal class VsoNullLoggingScope : IDisposable
    {
        public static VsoNullLoggingScope Instance { get; } = new VsoNullLoggingScope();

        private VsoNullLoggingScope() { }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
