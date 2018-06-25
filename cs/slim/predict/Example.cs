using System.Collections.Generic;
using System.Linq;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// A VowpalWabbit example. Basically a list of namespaces along with features.
    /// </summary>
    public class Example
    {
        public Dictionary<ushort, List<Feature>> Namespaces { get; set; }

        /// <summary>
        /// Merges namespaces of feature of another example with the current.
        /// </summary>
        /// <param name="other">The example to merge into this.</param>
        /// <returns>Either returns a new example if other is not null, or this.</returns>
        public Example Merge(Example other)
        {
            if (other == null)
                return this;

            // copy so we can add shared features
            var ex = new Example { Namespaces = new Dictionary<ushort, List<Feature>>(this.Namespaces) };

            foreach (var ns in other.Namespaces)
            {
                List<Feature> existingFeatures;
                if (ex.Namespaces.TryGetValue(ns.Key, out existingFeatures))
                {
                    // copy the list
                    existingFeatures = existingFeatures.Union(ns.Value).ToList();
                    ex.Namespaces[ns.Key] = existingFeatures;
                }
                else
                    ex.Namespaces.Add(ns.Key, ns.Value);
            }

            return ex;
        }
    }
}