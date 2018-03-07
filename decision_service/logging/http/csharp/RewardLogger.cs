using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace Microsoft.DecisionService.Logging
{
    public class RewardLogger : IDisposable
    {
        private EventHubSendClient eventHubSendClient;

        public RewardLogger(string eventHubConnectionString)
        {
            this.eventHubSendClient = new EventHubSendClient(eventHubConnectionString);
        }

        public Task RewardAsync(string eventId, float reward, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.RewardAsync(eventId, reward.ToString(CultureInfo.InvariantCulture), cancellationToken);
        }

        public Task RewardAsync(string eventId, string reward, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new StringBuilder();
            buffer.Append('{')
                  // TODO: escaping?
                  .Append("\"EventId\":\"").Append(eventId).Append("\",")
                  .Append("\"v\":").Append(reward)
                  .Append('}');

            // SendAsync (and the underlying PostAsync) is thread-safe
            // https://msdn.microsoft.com/en-us/library/system.net.http.httpclient(v=vs.110).aspx#Anchor_5
            return this.eventHubSendClient.SendAsync(buffer.ToString(), cancellationToken);
        }

        public void Dispose()
        {
            this.eventHubSendClient?.Dispose();
            this.eventHubSendClient = null;
        }
    }
}