using System;
using System.Collections.Concurrent;
using System.Threading;

namespace THNETII.AzureDevOps.Pipelines.Logging
{
    public class VsoConsoleProcessor : IDisposable
    {
        protected const int maxQueuedMessages = 1024;
        private readonly BlockingCollection<string> messageQueue =
            new BlockingCollection<string>(maxQueuedMessages);
        private readonly Thread writerThread;

        public VsoConsoleProcessor()
        {
            writerThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "VSO Console logger queue processing thread"
            };
            writerThread.Start();
        }

        public virtual void EnqueueMessage(string message)
        {
            if (!messageQueue.IsAddingCompleted)
            {
                try
                {
                    messageQueue.Add(message);
                }
                catch (InvalidOperationException) { }
            }
            else
            {
                // Adding is completed so just log the message
                try
                {
                    WriteMessage(message);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception) { }
#pragma warning restore CA1031 // Do not catch general exception types 
            }
        }

        protected virtual void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }

        private void ProcessLogQueue()
        {
            try
            {
                foreach (var message in messageQueue.GetConsumingEnumerable())
                {
                    WriteMessage(message);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                try
                {
                    messageQueue.CompleteAdding();
                }
                catch { }
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        protected virtual void Dispose(bool disposing)
        {
            messageQueue.CompleteAdding();
            try
            {
                writerThread.Join();
            }
            catch (ThreadStateException) { }

            messageQueue.Dispose();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~VsoConsoleProcessor() => Dispose(disposing: false);
    }
}
