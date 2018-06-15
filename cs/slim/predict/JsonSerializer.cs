using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;

namespace VowpalWabbit.Prediction
{
    public static class JsonSerializer
    {
        public static string ToJson(this Example ex)
        {
            var json = new StringBuilder();
            json.Append('{')
             .Append(string.Join(",",
                ex.Namespaces.Select(kv =>
                 string.Format("\"{0}\":{{{1}}}",
                     kv.Key, string.Join(",", kv.Value.Select(f => f.ToJson()))))))
             .Append('}');
            //json.Append('{')
            //    .Append(string.Join(",",
            //       this.Namespaces.Select((k, i) =>
            //        string.Format("\"{0}\":{1}",
            //            i, string.Join(",", k.Select(f => f.ToJson()))))))
            //    .Append('}');

            return json.ToString();
        }

        public static string ToJson(this Feature f)
        {
            var sb = new StringBuilder();

            sb.Append('\"').Append(f.WeightIndex).Append("\":");
            sb.Append(f.X.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }
    }
}
