using System;
using System.Collections.Generic;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// A parsed Vowpal Wabbit model.
    /// </summary>
    public class Model
    {
        private float lambda;

        private int numBits;

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

        private string commandlineArguments;

        public string CommandlineArguments
        {
            get { return this.commandlineArguments; }

            set
            {
                this.commandlineArguments = value;

                this.IsCbAdfExplore = value.Contains("--cb_explore_adf");
                this.IsCsoaaLdf = value.Contains("--csoaa_ldf");
            }
        }

        public List<string> Interactions { get; set; }

        public bool[] IgnoreLinear { get; set; }

        public bool NoConstant { get; set; }  

        public int NumBits
        {
            get { return this.numBits; }
            set
            {
                this.numBits = value;
                this.WeightMask = (1ul << value) - 1ul;
            }
        }

        public bool IsCbAdfExplore { get; private set; }

        public bool IsCsoaaLdf { get; private set; }

        public UInt64 WeightMask { get; private set; }

        public UInt64 GetStridedIndex(UInt64 idx, ulong offset)
            // apply strideshift for multiple models
            // select model
            // apply weightMask to stay within bounds
            => ((idx << this.StrideShift) + offset) & this.WeightMask;

        public float GetWeight(UInt64 idx, ulong offset)
            => this.Weights[this.GetStridedIndex(idx, offset)];

    }
}