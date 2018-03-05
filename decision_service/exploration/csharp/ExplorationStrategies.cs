using System;
using System.Linq;

namespace Microsoft.DecisionService
{
    /// <summary>
    /// Exploration strategies
    /// </summary>
    public static class ExplorationStrategies
    {
        /// <summary>
        /// Epsilon-greedy exploration
        /// </summary>
        /// <param name="epsilon">probability of exploration each action.</param>
        /// <param name="topAction">Index of the top action receiving 1-epsilon of the probability.</param>
        /// <param name="numActions">Total number of actions.</param>
        /// <returns></returns>
        public static float[] EpsilonGreedy(float epsilon, int topAction, int numActions)
        {
            if (numActions <= 0)
                return new float[0];

            if (topAction >= numActions)
                throw new ArgumentOutOfRangeException("topAction", "topAction must be smaller than numActions");

            float prob = epsilon / numActions;

            var probabilityDistribution = new float[numActions];
            for (int i = 0; i < probabilityDistribution.Length; i++)
                probabilityDistribution[i] = prob;

            probabilityDistribution[topAction] += 1f - epsilon;

            return probabilityDistribution;
        }

        public static float[] Softmax(float lambda, float[] scores)
        {
            float norm = 0;
            float maxScore = scores.Max();

            var probabilityDistribution = new float[scores.Length];
            for (int i = 0; i < probabilityDistribution.Length; i++)
            {
                float prob = (float)Math.Exp(lambda * (scores[i] - maxScore));
                norm += prob;

                probabilityDistribution[i] = prob;
            }

            // normalize
            if (norm > 0)
                for (int i = 0; i < probabilityDistribution.Length; i++)
                    probabilityDistribution[i] /= norm;

            return probabilityDistribution;
        }

        public static float[] Bag(int[] topActions)
        {
            if (topActions.Length == 0)
                throw new ArgumentOutOfRangeException("topActions", "must supply at least one topActions from a model");

            // determine probability per model
            float prob = 1f / (float)topActions.Length;

            var probabilityDistribution = new float[topActions.Length];

            for (int i = 0; i < topActions.Length; i++)
                probabilityDistribution[topActions[i]] += prob;

            return probabilityDistribution;
        }

        public static void EnforceMinimumProbability(float minProb, float[] probabilityDistribution)
        {
            bool zeros = false;

            //input: a probability distribution
            //output: a probability distribution with all events having probability > min_prob.  This includes events with probability 0 if zeros = true
            if (minProb > 0.999) // uniform exploration
            {
                int supportSize = probabilityDistribution.Length;
                if (!zeros)
                    supportSize -= probabilityDistribution.Count(p => p == 0);

                for (int i = 0; i < probabilityDistribution.Length; i++)
                {
                    if (zeros || probabilityDistribution[i] > 0)
                        probabilityDistribution[i] = 1f / supportSize;
                }

                return;
            }

            minProb /= probabilityDistribution.Length;
            float touched_mass = 0;
            float untouched_mass = 0;

            for (int i = 0; i < probabilityDistribution.Length; i++)
            {
                float prob = probabilityDistribution[i];

                if ((prob > 0 || (prob == 0 && zeros)) && prob <= minProb)
                {
                    touched_mass += minProb;
                    prob = minProb;
                }
                else
                    untouched_mass += prob;
            }

            if (touched_mass > 0)
            {
                if (touched_mass > 0.999)
                    throw new ArgumentException("Cannot safety this distribution");

                float ratio = (1f - touched_mass) / untouched_mass;
                for (int i = 0; i < probabilityDistribution.Length; i++)
                    if (probabilityDistribution[i] > minProb)
                            probabilityDistribution[i] *= ratio;
            }
        }
    }
}
