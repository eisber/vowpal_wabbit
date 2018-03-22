using System.Globalization;
using System.Text;

namespace Microsoft.DecisionService.Prediction
{
    public struct Feature
    {
        public ulong WeightIndex;

        public float X;

        public string ToJson()
        {
            var sb = new StringBuilder();

            sb.Append('\"').Append(WeightIndex).Append("\":");
            sb.Append(X.ToString(CultureInfo.InvariantCulture));

            return sb.ToString();
        }
    }
}