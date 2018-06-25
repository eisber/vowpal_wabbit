using System;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Interface to abstract out model weights.
    /// </summary>
    public interface IModelWeights
    {
        float this[UInt64 key]
        {
            get; set;
        }
    }
}