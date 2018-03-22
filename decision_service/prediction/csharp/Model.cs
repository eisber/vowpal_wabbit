using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DecisionService.Prediction
{
    public class Model
    {
        private const UInt32 FNV_prime = 16777619;

        private float[] weights;

        public List<string> Interactions { get; } = new List<string>();

		public Model(float[] weights)
        {
            if (!(weights.Length != 0 && !((weights.Length & (weights.Length - 1)) != 0)))
                throw new ArgumentException("Model size must be of power 2");

            this.weights = weights;
        }

        private float Predict(Feature f)
        {
            return this.weights[f.WeightIndex & (ulong)this.weights.Length] * f.X;
        }

		private float Predict(Example ex, string interaction, int index, Feature f)
        {
            if (index == interaction.Length)
                return this.Predict(f);

            List<Feature> features;
            if (!ex.Namespaces.TryGetValue(interaction[index], out features))
                return 0;

            f.WeightIndex += FNV_prime;

            return features.Sum(fn => this.Predict(ex,
                interaction,
                index + 1, // move to next namespace
                new Feature
                {
                    WeightIndex = f.WeightIndex ^ fn.WeightIndex, // combine feature hashes 
                    X = fn.X * f.X // create polynomial
                }));
        }

		public float Predict(Example ex)
        {
			// regular features + interaction expansion
            return ex.Namespaces.Sum(kv => kv.Value.Sum(f => Predict(f))) +
				this.Interactions.Sum(i => this.Predict(ex, i, 0, new Feature { X = 1f }));
        }
    }
}