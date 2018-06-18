using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VowpalWabbit.Prediction
{
    public interface IModelWeights
    {
        float this[UInt64 key]
        {
            get; set;
        }
    }

    public sealed class DenseModelWeights : IModelWeights
    {
        private readonly float[] weights;

        public DenseModelWeights(int numBits)
        {
            this.weights = new float[1 << numBits];
        }

        public float this[UInt64 key] 
        { 
            // TODO: conversions are fine?
            get { return this.weights[(int)key]; }
            set { this.weights[(int)key] = value; }
        }
    }

    public sealed class SparseModelWeights : IModelWeights
    {
        private readonly Dictionary<UInt64, float> weights = new Dictionary<UInt64, float>();

        public float this[UInt64 key]
        { 
            get
            {
                // TODO: stride shift
                float value;
                if (!this.weights.TryGetValue(key, out value))
                    value = 0;

                return value;
            } 
            
            set { this.weights[key] = value; }
        }
    }

    public class Model
    {
        private float lambda;

        public IModelWeights Weights { get; set; }

        public Exploration Exploration { get; set; }

        public float Epsilon { get; set; }

        public float BagMinimumEpsilon { get; set; }

        public int BagSize { get; set; }

        public float Lambda 
        {
            get { return this.lambda; }
            set
            {
                // Lambda should always be negative because we are using a cost basis.
                this.lambda = value > 0 ? -value : value;
            }
        }

        public int StrideShift { get; set; }

        public string Id { get; set; }

        public string CommandlineArguments { get; set; }

        public List<string> Interactions { get; set; }

        public bool[] IgnoreLinear { get; set; }

        public bool NoConstant { get; set; }  

        public int NumBits { get; set; }

        // this needs the "hashing" mode
        public void ParseNamespace(string s, out ushort featureGroup, out UInt64 hash)
        {
            featureGroup = (ushort)s[0];
            hash = MurMurHash3.ComputeIdHash(s, 0);
        }

        public UInt64 ParseFeature(string s, UInt64 namespaceHash)
        {
            UInt64 idx;
            if (ulong.TryParse(s, out idx))
                return namespaceHash + idx;

            return MurMurHash3.ComputeIdHash(s, (uint)namespaceHash);
        }
    }
}