using System;
using System.Net.Http.Headers;

namespace Microsoft.DecisionService.Logging
{
    public class SasTokenProvider
    {
        public SasTokenProvider(string resourceUri, string keyName, string key)
        {
            this.ResourceUri = resourceUri;
            this.KeyName = keyName;
            this.Key = key;
        }

        public string ResourceUri
        {
            get;
        }

        public string KeyName
        {
            get;
        }

        private string Key
        {
            get;
        }

        public DateTime Expiry
        {
            get;
            private set;
        } = DateTime.MinValue;

        public bool IsExpired => (this.Expiry - DateTime.UtcNow).TotalHours < 1;

        public string TokenString
        {
            get;
            private set;
        }

        public AuthenticationHeaderValue ToHeaderValue()
        {
            return new AuthenticationHeaderValue("SharedAccessSignature", this.TokenString);
        }

        public void RegenerateToken()
        {
            DateTime expiryUtc = DateTime.UtcNow.AddDays(7);

            this.TokenString = SasTokenHelper.CreateToken(this.ResourceUri, this.KeyName, this.Key, expiryUtc);
            this.Expiry = expiryUtc;
        }
    }
}
