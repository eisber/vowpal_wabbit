using System.Collections.Generic;
using System.Linq;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// Extensions methods to help featurization of examples
    /// </summary>
    public static class AuditExtensions
    {
        /// <summary>
        /// Returns full expanded feature hashes and names.
        /// </summary>
        /// <param name="example">The example to be audited.</param>
        /// <param name="m">The model to audit this against.</param>
        /// <returns></returns>
        public static IEnumerable<AuditFeature> Audit(this MultilineExample example, Model m)
        {
            if (m.IsCbAdfExplore && m.Exploration == Exploration.Bag)
                return Enumerable.Range(0, m.BagSize)
                    .SelectMany(offset => example.Audit(m, (ulong)offset));

            return example.Audit(m, 0);
        }

        private static IEnumerable<AuditFeature> Audit(this Example ex, Model m, string interaction, int index, AuditFeature f, ulong offset)
        {
            if (index == interaction.Length)
            {
                if (f.Name.Length > 0)
                    yield return f;
                yield break;
            }

            if (!ex.Namespaces.TryGetValue(interaction[index], out var features))
                yield break;

            f.WeightIndex *= Constants.FNV_prime;

            var sub = features.SelectMany(fo => ex.Audit(m, interaction, index + 1,
                new AuditFeature
                {
                    Name = f.Name + "^" + (fo as AuditFeature)?.Name,
                    Namespace = f.Namespace + "^" + (fo as AuditFeature)?.Namespace,
                    WeightIndex = f.WeightIndex ^ fo.WeightIndex
                }, offset));

            foreach (var item in sub)
                yield return item;
        }

        private static IEnumerable<AuditFeature> Audit(this MultilineExample example, Model m, ulong offset)
        {
            // TODO: add support for constant
            return example.Examples
                .Select(ex => ex.Merge(example.Shared))
                .SelectMany(ex =>
                    // linear terms
                    ex.Namespaces.SelectMany(ns =>
                        ns.Value.Select(f =>
                            new AuditFeature
                            {
                                WeightIndex = ((f.WeightIndex << m.StrideShift) + offset) & m.WeightMask,
                                Name = (f as AuditFeature)?.Name,
                                Namespace = (f as AuditFeature)?.Namespace
                            }))
                    // interactions
                    .Union(m.Interactions
                            .SelectMany(i => ex.Audit(m, i, 0, new AuditFeature { Name = "", Namespace = "" }, offset))
                            .Select(f =>
                            {
                                // Apply masking
                                f.WeightIndex = ((f.WeightIndex << m.StrideShift) + offset) & m.WeightMask;
                                return f;
                            })));
        }
    }
}
