using System;
using System.Collections.Generic;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Implementation of sparse model weight backend.
    /// </summary>
    public sealed class SparseModelWeights : IModelWeights
    {
        public readonly Dictionary<UInt64, float> Weights = new Dictionary<UInt64, float>();

        public float this[UInt64 key]
        { 
            get
            {
                float value;
                if (!this.Weights.TryGetValue(key, out value))
                    value = 0;

                return value;
            } 
            
            set { this.Weights[key] = value; }
        }
    }
}