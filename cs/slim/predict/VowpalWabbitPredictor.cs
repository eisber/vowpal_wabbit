using System;
using System.Collections.Generic;
using System.Linq;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Starting point to get various predictor implementations.
    /// </summary>
    public class VowpalWabbitPredictor
    {
        /// <summary>
        /// Instantiates the various predictors
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static VowpalWabbitPredictor Create(Model model)
        {
            if (model.IsCbAdfExplore)
            {
                switch (model.Exploration)
                {
                    case Exploration.EpsilonGreedy:
                        return new VowpalWabbitPredictorContextualBanditEpsilonGreedy(model);

                    case Exploration.Softmax:
                        return new VowpalWabbitPredictorContextualBanditSoftmax(model);

                    case Exploration.Bag:
                        return new VowpalWabbitPredictorContextualBanditBag(model);

                    default:
                        throw new NotSupportedException("Exploration: " + model.Exploration + " is not supported");
                }
            }

            if (model.IsCsoaaLdf)
                return new VowpalWabbitPredictorMulticlass(model);

            return new VowpalWabbitPredictorRegression(model);
        }

        private static readonly Feature ConstantFeature = new Feature { WeightIndex = Constants.constant, X = 1f };

        protected readonly Model model;

        protected VowpalWabbitPredictor(Model model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (model.Weights == null)
                throw new ArgumentNullException("model.Weights");

            this.model = model;
        }

#if NET40
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        protected float Predict(Feature f, ulong offset)
        {
            return this.model.GetWeight(f.WeightIndex, offset) * f.X;
        }

        private float Predict(Example ex, string interaction, int index, Feature f, ulong offset)
        {
            if (index == interaction.Length)
                return this.Predict(f, offset);

            if (!ex.Namespaces.TryGetValue(interaction[index], out var features))
                return 0;

            f.WeightIndex *= Constants.FNV_prime;

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

        protected float Predict(Example ex, ulong offset = 0)
        {
            List<Feature> constantNamespace = null;

            try
            {
                // Note: the current models don't store --noconstant, just the weights will be 0
                // to be forward compatible, let's still implement it
                if (!this.model.NoConstant)
                {
                    if (!ex.Namespaces.TryGetValue(Constants.constant_namespace, out constantNamespace))
                    {
                        constantNamespace = new List<Feature> { ConstantFeature };
                        ex.Namespaces.Add(Constants.constant_namespace, constantNamespace);
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

        protected float[] Predict(MultilineExample example, ulong offset = 0)
        {
            var preds = new float[example.Examples.Count];

            int i = 0;
            foreach (var ex in example.Examples)
            {
                var merged = ex.Merge(example.Shared);

                preds[i++] = this.Predict(merged, offset);
            }

            return preds;
        }
    }
}
