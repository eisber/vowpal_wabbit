using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VowpalWabbit.Prediction
{
    /// <summary>
    /// VowpalWabbit text format parser (no label support).
    /// </summary>
    public static class TextDeserializer
    {
        /// <summary>
        /// Parses VowpalWabbit test format.
        /// </summary>
        /// <param name="line">The data.</param>
        /// <returns>Parsed example.</returns>
        public static Example ParseExample(string line)
        {
            // An example line: 
            // 1 |a 0:1 |b 2:2

            var ex = new Example { Namespaces = new Dictionary<ushort, List<Feature>>() };

            // get namespaces, skip label
            foreach (var ns in line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                // split features
                var fields = ns.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray(); 

                int fieldStart = 0;
                string namespaceValue;
                if (ns.StartsWith(" "))
                    namespaceValue = " ";
                else
                {
                    namespaceValue = fields[0];
                    fieldStart = 1;
                }

                ushort featureGroup;
                UInt64 namespaceHash;
                HashUtil.ParseNamespace(namespaceValue, out featureGroup, out namespaceHash);

                ex.Namespaces.Add(featureGroup,
                    fields.Skip(fieldStart)
                        .Select(pairs => pairs.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
                            // split 0:1
                            .Select(s =>
                                new AuditFeature
                                {
                                    WeightIndex = HashUtil.ParseFeature(s[0], namespaceHash),
                                    X = float.Parse(s[1], NumberStyles.Any, CultureInfo.InvariantCulture),
                                    Name = s[0],
                                    Namespace = namespaceValue
                                })
                            .ToList<Feature>());
            }

            return ex;
        }
    }
}
