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

            int offset = 0;
            int end = s.Length;
            if (end == 0)
            {
                hash = 0;
                return;
            }

            //trim leading whitespace but not UTF-8
            for (; offset < s.Length && s[offset] <= 0x20; offset++) ;
            for (; end >= offset && s[end - 1] <= 0x20; end--) ;

            int length = end - offset;
            if (length <= 0)
            {
                hash = 0;
                return;
            }

            hash = MurMurHash3.ComputeIdHash(s.Substring(offset, length), 0);
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
