using System.Diagnostics;

namespace VowpalWabbit.Prediction
{
    [DebuggerDisplay("Feature({WeightIndex}:{X})")]
    public struct Feature
    {
        public ulong WeightIndex;

        public float X;
    }
}