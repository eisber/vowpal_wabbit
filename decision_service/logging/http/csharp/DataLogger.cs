using System;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Microsoft.DecisionService.Logging
{
    public class DataLogger : IDisposable
    {
        private readonly Queue<string> queue = new Queue<string>();
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly Task backgroundTask;
        private EventHubSendClient eventHubSendClient;

        public DataLogger(string eventHubConnectionString)
        {
            this.backgroundTask = Task.Run(this.BackgroundUploadAsync);
            this.eventHubSendClient = new EventHubSendClient(eventHubConnectionString);
        }

        public TimeSpan BatchingTimeout { get; set; } = TimeSpan.FromSeconds(5);

        public Action<Exception> BackgroundExceptionListener { get; set; } 

        private async Task<bool> BatchAndSendAsync(StringBuilder buffer)
        {
            try
            {
                buffer.Clear();

                lock (queue)
                {
                    var batchSize = 0;

                    while (queue.Count > 0)
                    {
                        var elem = queue.Peek();
                        var elemSize = Encoding.UTF8.GetMaxByteCount(elem.Length);

                        // EventHub size restriction
                        if (batchSize + elemSize >= 256 * 1024)
                            break;

                        if (batchSize > 0)
                        {
                            buffer.Append('\n');
                            batchSize++;
                        }

                        buffer.Append(elem);
                        batchSize += elemSize;

                        queue.Dequeue();
                    }
                }

                if (buffer.Length == 0)
                    return false;

                // don't want to pass our cancellation token as it's used to control draining
                await this.eventHubSendClient.SendAsync(buffer.ToString());

                return true;
            }
            catch (Exception ex)
            {
                this.BackgroundExceptionListener?.Invoke(ex);

                return false;
            }
        }

        private async Task BackgroundUploadAsync()
        {
            var buffer = new StringBuilder();

            try
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    if (!await this.BatchAndSendAsync(buffer))
                        await Task.Delay(this.BatchingTimeout, this.cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            { }

            // drain the buffer
            while (await this.BatchAndSendAsync(buffer)) ;
        }

        public async Task StopAndDrainAsync()
        {
            // stop the batching
            this.cancellationTokenSource.Cancel();

            // wait for the queue to be drained
            await this.backgroundTask;
        }

        // full distribution or numActions
        public void Log(string eventId, string context, float[] probabilities, int[] ranking, string modelVersion)
        {
            var sb = new StringBuilder();

            // Version needs to be upfront for parsing
            sb.Append("{\"Version\":\"1\",")
              // TODO: escaping?
              .Append("\"EventId\":\"").Append(eventId).Append("\",")
              // Actions are 1-based
              .Append("\"a\":[").Append(string.Join(",", ranking.Select(i => (i + 1).ToString(CultureInfo.InvariantCulture)))).Append("],")
              .Append("\"c\":").Append(context).Append(',')
              .Append("\"p\":[").Append(string.Join(",", probabilities.Select(i => i.ToString(CultureInfo.InvariantCulture)))).Append("],")
              // TODO: escaping?
              .Append("\"m\":\"").Append(modelVersion).Append('\"')
              .Append('}');

            var str = sb.ToString();

            lock (queue)
            {
                queue.Enqueue(str);
            }
        }

        public void Dispose()
        {
            this.eventHubSendClient?.Dispose();
            this.eventHubSendClient = null;
        }
    }
}