using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace VowpalWabbit.Prediction
{
    [DebuggerDisplay("Feature({WeightIndex}:{X})")]
    public struct Feature
    {
        public ulong WeightIndex;

        public float X;
    }
}