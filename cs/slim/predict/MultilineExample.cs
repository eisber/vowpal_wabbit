using System.Collections.Generic;

namespace VowpalWabbit.Prediction
{
    public class MultilineExample
    {
        public Example Shared { get; set; }

        public IReadOnlyCollection<Example> Examples { get; set; }
    }
}