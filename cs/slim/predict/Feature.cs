using System.Diagnostics;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// A single sparse feature.
    /// </summary>
    [DebuggerDisplay("Feature({WeightIndex}:{X})")]
    public class Feature
    {
        public ulong WeightIndex;

        public float X;
    }
}