using Microsoft.DecisionService.Exploration;
using System;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Helper functions to perform feature hashing.
    /// </summary>
    public static class HashUtil
    {
        public static void ParseNamespace(string s, out ushort featureGroup, out UInt64 hash)
        {
            featureGroup = (ushort)s[0];
            hash = MurMurHash3.ComputeIdHash(s, 0);
        }

        public static UInt64 ParseFeature(string s, UInt64 namespaceHash)
        {
            UInt64 idx;
            if (ulong.TryParse(s, out idx))
                return namespaceHash + idx;

            return MurMurHash3.ComputeIdHash(s, (uint)namespaceHash);
        }
    }
}
