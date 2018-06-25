using System.Text;
using System.Linq;
using System.Globalization;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Serializes examples into JSON format.
    /// </summary>
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

            return json.ToString();
        }

        public static string ToJson(this Feature f)
        {
            var sb = new StringBuilder();

            sb.Append('\"').Append(f.WeightIndex).Append("\":");
            sb.Append(f.X.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }

        public static string ToJson(this MultilineExample ex)
        {
            var json = new StringBuilder();
            json.Append('{');
            if (ex.Shared != null)
                json.Append("\"shared\":").Append(ex.Shared.ToJson()).Append(',');

            json.Append("\"_multi\":[")
                .Append(string.Join(",", ex.Examples.Select(a => a.ToJson())))
                .Append(']');

            json.Append('}');

            return json.ToString();
        }
    }
}
