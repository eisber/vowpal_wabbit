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
            this.weightMask = ((1ul << model.NumBits) << model.StrideShift) - 1ul;
            // System.Diagnostics.Debug.WriteLine("weightMask: " + this.weightMask);
        }

#if NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private float Predict(Feature f, ulong offset)
        {
            // System.Diagnostics.Debug.WriteLine("StrideShift: " + model.StrideShift);
            // apply strideshift for multiple models
            UInt64 idx = f.WeightIndex << this.model.StrideShift;
            // select model
            idx += offset;
            // apply weightMask to stay within bounds
            idx &= this.weightMask;

            // System.Diagnostics.Debug.WriteLine("Predict: " + f.WeightIndex +"/" + idx + ":" + f.X + "*" + this.model.Weights[idx]);

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
                // Note: the current models don't store --noconstant, just the weights will be 0
                // to be forward compatible, let's still implement it
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

                // compute linear terms
                float pred = ex.Namespaces
                    .Where(kv => !this.model.IgnoreLinear[kv.Key])
                    .Sum(kv => kv.Value.Sum(f => Predict(f, offset)));

                // compute interaction terms
                pred += this.model.Interactions.Sum(i => this.Predict(ex, i, 0, new Feature { X = 1f }, offset));

                return pred;

            }
            finally
            {
                // don't remove namespace, just clear it
                constantNamespace?.Clear();
            }
        }

        public float[] Predict(MultilineExample example)
        {
            var preds = new float[example.Examples.Count];

            int i = 0;
            foreach (var ex in example.Examples)
            {
                var ex2 = ex;

                if (example.Shared != null)
                {
                    // copy so we can add shared features
                    ex2 = new Example { Namespaces = new Dictionary<ushort, List<Feature>>(ex.Namespaces) };

                    foreach (var ns in example.Shared.Namespaces)
                    {
                        List<Feature> existingFeatures;
                        if (ex2.Namespaces.TryGetValue(ns.Key, out existingFeatures))
                        {
                            // copy the list
                            existingFeatures = existingFeatures.Union(ns.Value).ToList();
                            ex2.Namespaces[ns.Key] = existingFeatures;
                        }
                        else
                            ex2.Namespaces.Add(ns.Key, ns.Value);
                    }
                }

                preds[i++] = this.Predict(ex2);
            }

            return preds;
        }
    }
}
