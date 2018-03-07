using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.DecisionService.Logging
{
    public class EventHubSendClient : IDisposable
    {
        private readonly string path;
        private HttpClient httpClient;

        public EventHubSendClient(string connectionString)
        {
            this.Connection = EventHubConnection.FromConnectionString(connectionString);

            this.SasTokenProvider = new SasTokenProvider($"https://{this.Connection.Hostname}/{this.Connection.EntityPath}", this.Connection.SasKeyName, this.Connection.SasKey);
            this.SasTokenProvider.RegenerateToken();

            this.httpClient = new HttpClient()
            {
                BaseAddress = new Uri($"https://{this.Connection.Hostname}") 
            };
            this.httpClient.DefaultRequestHeaders.Authorization = this.SasTokenProvider.ToHeaderValue();

            this.path = $"{this.Connection.EntityPath}/messages?timeout=60";
        }

        private EventHubConnection Connection
        {
            get;
            set;
        }

        private SasTokenProvider SasTokenProvider
        {
            get;
            set;
        }

        private void EnsureValidToken()
        {
            if (this.SasTokenProvider.IsExpired)
            {
                this.SasTokenProvider.RegenerateToken();
                this.httpClient.DefaultRequestHeaders.Authorization = this.SasTokenProvider.ToHeaderValue();
            }
        }

        private int NumberOfRetries { get; set; } = 5;

        public async Task SendAsync(string body, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpResponseMessage response = null;
            for (int i = 0; i < this.NumberOfRetries; i++)
            {
                this.EnsureValidToken();

                var content = new StringContent(body, Encoding.UTF8);
                content.Headers.TryAddWithoutValidation("Content-Type", "application/atom+xml;type=entry;charset=utf-8");
                response = await this.httpClient.PostAsync(
                    this.path,  
                    content, 
                    cancellationToken)
                    .ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                    return;

                // exponential backoff
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
            }

            throw new Exception($"Unable to send to EventHub. Status Code: {response?.StatusCode}: {response?.ReasonPhrase}");
        }

        public void Dispose()
        {
            this.httpClient?.Dispose();
            this.httpClient = null;
        }
    }
}
