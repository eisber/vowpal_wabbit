using System.Globalization;
using System.Linq;
using System.Text;
using System.Diagnostics;

[assembly: DebuggerDisplay(@"Example: {TextSerializer.ToText(this)}", Target = typeof(VowpalWabbit.Prediction.Example))]

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Serializes a example to it's corresponding text representation.
    /// </summary>
    public static class TextSerializer
    {
        private static string ToText(this Feature f)
        {
            var sb = new StringBuilder();

            if (f is AuditFeature full)
                sb.Append(full.Name);
            else
                sb.Append(f.WeightIndex).Append(':');

            sb.Append(f.X.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }

        /// <summary>
        /// Serializes an example to VowpalWabbit text format.
        /// </summary>
        /// <param name="ex">The example.</param>
        /// <returns>The serialized string.</returns>
        public static string ToText(this Example ex)
        {
            var text = new StringBuilder();
            text
             .Append(string.Join(" ",
                ex.Namespaces.Select(kv =>
                {
                    // let's check if we actually have namespace information
                    var firstFull = kv.Value.First() as AuditFeature;

                    return string.Format("|{0} {1}",
                        firstFull?.Namespace ?? kv.Key.ToString(),
                        string.Join(" ", kv.Value.Select(f => f.ToText())));
                })));

            return text.ToString();
        }

        /// <summary>
        /// Serializes multiline examples to VowpalWabbit text format.
        /// </summary>
        /// <param name="ex">The multiline example.</param>
        /// <returns>The serialized string.</returns>
        public static string ToText(this MultilineExample ex)
        {
            var text = new StringBuilder();
            text.Append("shared ").Append(ToText(ex.Shared)).Append('\n')
                .Append(string.Join("\n", ex.Examples.Select(ToText)));
            
            return text.ToString();
        }
    }
}
