namespace Microsoft.DecisionService
{
    public class Sampling 
	{
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
    }
}
