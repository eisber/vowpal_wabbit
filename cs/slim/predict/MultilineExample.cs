using System.Collections.Generic;
using System.Diagnostics;

[assembly: DebuggerDisplay(@"MultilineExample: {TextSerializer.ToText(this)}", Target = typeof(VowpalWabbit.Prediction.MultilineExample))]

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// A multiline VowpalWabbit example.
    /// </summary>
    public class MultilineExample
    {
        /// <summary>
        /// The shared part of the multline example. These namespaces/features will be copied into each example.
        /// </summary>
        public Example Shared { get; set; }

        /// <summary>
        /// The actual data points.
        /// </summary>
        public IReadOnlyCollection<Example> Examples { get; set; }
    }
}