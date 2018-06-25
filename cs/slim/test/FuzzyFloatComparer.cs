using System;
using System.Collections;

namespace VowpalWabbit.Prediction.Tests
{
    public class FuzzyFloatComparer : IComparer
    {
        private readonly double epsilon;

        public FuzzyFloatComparer(double epsilon)
        {
            this.epsilon = epsilon;
        }

        public int Compare(object a, object b)
        {
            var x = (float)a;
            var y = (float)b;

            // from test/RunTests
            var delta = Math.Abs(x - y);

            if (delta > this.epsilon)
            {
                // We have a 'big enough' difference, but this difference
                // may still not be meaningful in all contexts:

                // Big numbers should be compared by ratio rather than
                // by difference

                // Must ensure we can divide (avoid div-by-0)
                if (Math.Abs(y) <= 1.0)
                {
                    // If numbers are so small (close to zero),
                    // ($delta > $Epsilon) suffices for deciding that
                    // the numbers are meaningfully different
                    return -1;
                }

                // Now we can safely divide (since abs($word2) > 0)
                // and determine the ratio difference from 1.0
                var ratio_delta = Math.Abs(x / y - 1.0);
                if (ratio_delta > this.epsilon)
                {
                    return 1;
                }
            }

            return 0;
        }
    }
}
