using System;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
#if NET40
using System.Web;
#endif

namespace Microsoft.DecisionService.Logging
{
    internal static class SasTokenHelper
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

        public static string CreateToken(string resourceUri, string keyName, string key, DateTime expirationUtc)
        {
            TimeSpan sinceEpoch = expirationUtc - SasTokenHelper.UnixEpoch;
            string expiry = Convert.ToString((int)sinceEpoch.TotalSeconds);

#if NET40
            string resourceUriEncoded = HttpUtility.UrlEncode(resourceUri);
#else
            string resourceUriEncoded = System.Net.WebUtility.UrlEncode(resourceUri);
#endif
            string stringToSign = resourceUriEncoded + "\n" + expiry;

            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            string signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

#if NET40
            string signatureEncoded = HttpUtility.UrlEncode(signature);
#else
            string signatureEncoded = System.Net.WebUtility.UrlEncode(signature);
#endif

            return $"sr={resourceUriEncoded}&sig={signatureEncoded}&se={expiry}&skn={keyName}";
        }
    }
}
