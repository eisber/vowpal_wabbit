using System;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Dense implementation for model weights.
    /// </summary>
    public sealed class DenseModelWeights : IModelWeights
    {
        private readonly float[] weights;

        public DenseModelWeights(int numBits)
        {
            this.weights = new float[1 << numBits];
        }

        public float this[UInt64 key] 
        { 
            get { return this.weights[(int)key]; }
            set { this.weights[(int)key] = value; }
        }
    }
}