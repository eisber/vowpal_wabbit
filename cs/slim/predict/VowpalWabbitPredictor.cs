using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace VowpalWabbit.Prediction
{
    public class VowpalWabbitPredictor
    {
        private static readonly Feature ConstantFeature = new Feature { WeightIndex = constant, X = 1f };

        // see constant.h
        private const ushort constant_namespace = 128;

        private const Int64 constant = 11650396;

        private const UInt32 FNV_prime = 16777619;

        private readonly Model model;

        private readonly UInt64 weightMask;

        public VowpalWabbitPredictor(Model model)
        {
            this.model = model;
            this.weightMask = (UInt64)(((1 << model.NumBits) << model.StrideShift) - 1);
        }

#if NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private float Predict(Feature f, ulong offset)
        {
            // apply strideshift for multiple models
            UInt64 idx = f.WeightIndex << this.model.StrideShift;
            // select model
            idx += offset;
            // apply weightMask to stay within bounds
            idx &= this.weightMask;

            return this.model.Weights[idx] * f.X;
        }

        private float Predict(Example ex, string interaction, int index, Feature f, ulong offset)
        {
            if (index == interaction.Length)
                return this.Predict(f, offset);

            if (!ex.Namespaces.TryGetValue(interaction[index], out var features))
                return 0;

            f.WeightIndex *= FNV_prime;

            return features.Sum(fn => this.Predict(ex,
                interaction,
                index + 1, // move to next namespace
                new Feature
                {
                    WeightIndex = f.WeightIndex ^ fn.WeightIndex, // combine feature hashes 
                    X = fn.X * f.X // create polynomial
                },
                offset));
        }

        public float Predict(Example ex, ulong offset = 0)
        {
            List<Feature> constantNamespace = null;

            try
            {
                // handle constant
                if (!this.model.NoConstant)
                {
                    if (!ex.Namespaces.TryGetValue(constant_namespace, out constantNamespace))
                    {
                        constantNamespace = new List<Feature> { ConstantFeature };
                        ex.Namespaces.Add(constant_namespace, constantNamespace);
                    }
                    else
                        constantNamespace.Add(ConstantFeature);
                }

                // regular features + interaction expansion
                return ex.Namespaces.Sum(kv => kv.Value.Sum(f => Predict(f, offset))) +
                    this.model.Interactions.Sum(i => this.Predict(ex, i, 0, new Feature { X = 1f }, offset));
            }
            finally
            {
                // don't remove namespace, just clear it
                constantNamespace?.Clear();
            }
        }
    }
}
