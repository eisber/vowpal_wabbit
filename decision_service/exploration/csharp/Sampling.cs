using System;
using System.Collections;
using System.Linq;

namespace Microsoft.DecisionService.Exploration
{
    public struct Sample
    {
        public float Probability;

        public int Index;
    }

    public class Sampling 
	{
        public static Sample Sample(string seed, float[] probabilityDistribution)
        {
            ulong hash = MurMurHash3.ComputeIdHash(seed);
            float randomDraw = PRG.UniformUnitInterval(hash);
            return Sample(randomDraw, probabilityDistribution);
        }

        public static Sample Sample(double draw, float[] probabilityDistribution)
        {
            // Create a discrete_distribution based on the returned weights. This class handles the
            // case where the sum of the weights is < or > 1, by normalizing agains the sum.
            float total = 0;
            foreach(float prob in probabilityDistribution)
            {
                if (prob < 0)
                    throw new ArgumentOutOfRangeException("Probabilities must be non-negative.");
                total += prob;
            }
            if (total == 0)
                throw new ArgumentOutOfRangeException("At least one score must be positive.");

            draw = total * draw;
            if (draw > total) //make very sure that draw can not be greater than total.
                draw = total;

            float action_probability = 0;
            int action_index = probabilityDistribution.Length - 1;
            float sum = 0;
            for (int i = 0; i < probabilityDistribution.Length; i++)
            {
                sum += probabilityDistribution[i];
                if (sum > draw)
                {
                    action_index = i;
                    action_probability = probabilityDistribution[i] / total;
                    break;
                }
            }

            return new Sample
            {
                Probability = action_probability,
                Index = action_index
            };
        }

        /// <summary>
        /// Produce ranking
        /// </summary>
        public static int[] Sample(string seed, float[] probabilityDistribution, float[] scores)
        {
            return SwapTopSlot(scores, Sample(seed, probabilityDistribution).Index);
        }

        private sealed class IndexComparer : IComparer
        {
            internal float[] scores;

            int IComparer.Compare(object x, object y)
            {
                return scores[(int)y].CompareTo(scores[(int)x]);
            }
        }

        public static int[] SwapTopSlot(float[] scores, int chosenAction)
        {
            int[] ranking = Enumerable.Range(0, scores.Length).ToArray();

            // use .NET Standard compatible sorting
            Array.Sort(ranking, new IndexComparer { scores = scores });

            SwapTopSlot(ranking, chosenAction);

            return ranking;
        }

        public static void SwapTopSlot(int[] ranking, int chosenAction)
        {
            int temp = ranking[0];
            ranking[0] = ranking[chosenAction];
            ranking[chosenAction] = temp;
        }
    }
}
