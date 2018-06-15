using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VowpalWabbit.Prediction
{
    public static class TextSerializer
    {
        public static string ToText(this Example ex)
        {
            var json = new StringBuilder();
            json
             .Append(string.Join(" ",
                ex.Namespaces.Select(kv =>
                 string.Format("|{0} {1}",
                     kv.Key, string.Join(" ", kv.Value.Select(f => f.ToText()))))));

            return json.ToString();
        }

        public static string ToText(this Feature f)
        {
            var sb = new StringBuilder();

            sb.Append(f.WeightIndex).Append(':');
            sb.Append(f.X.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }
    }
}
