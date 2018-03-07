using System;
using System.Text.RegularExpressions;

namespace Microsoft.DecisionService.Logging
{
    public class EventHubConnection
    {
        public static EventHubConnection FromConnectionString(string connectionString)
        {
            Match match = Regex.Match(connectionString, "^Endpoint=sb://([^;]+)/;SharedAccessKeyName=([^;]+);SharedAccessKey=([^;]+);EntityPath=(.+)$");

            if (!match.Success)
                throw new ArgumentException("Unable to parse EventHub connection string", nameof(connectionString));

            return new EventHubConnection
            {
                Hostname = match.Groups[1].Value,
                EntityPath = match.Groups[4].Value,
                SasKeyName = match.Groups[2].Value,
                SasKey = match.Groups[3].Value
            };
        }

        public string Hostname
        {
            get;
            set;
        }

        public string EntityPath
        {
            get;
            set;
        }

        public string SasKeyName
        {
            get;
            set;
        }

        public string SasKey
        {
            get;
            set;
        }
    }
}
