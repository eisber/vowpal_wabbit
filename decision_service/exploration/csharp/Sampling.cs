using System;
using System.Collections;
using System.Linq;

namespace Microsoft.DecisionService.Exploration
{
    public class Sampling 
	{
        public static int Sample(string seed, float[] probabilityDistribution)
        {
            ulong hash = MurMurHash3.ComputeIdHash(seed);
            float randomDraw = PRG.UniformUnitInterval(hash);
            return Sample(randomDraw, probabilityDistribution);
        }

        public static int Sample(double randomDraw, float[] probabilityDistribution)
        {
            float cumulativeSum = 0f;
            for (int i = 0; i < probabilityDistribution.Length; i++)
            {
                cumulativeSum += probabilityDistribution[i];
                // This needs to be >=, not >, in case the random draw = 1.0, since sum would never
                // be > 1.0 and the loop would exit without assigning the right action probability.
                if (cumulativeSum >= randomDraw)
                    return i;
            }

            return probabilityDistribution.Length - 1;
        }

        private sealed class IndexComparer : IComparer
        {
            internal float[] scores;

            int IComparer.Compare(object x, object y)
            {
                return scores[(int)x].CompareTo(scores[(int)y]);
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
