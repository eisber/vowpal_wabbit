using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VowpalWabbit.Prediction
{
    public static class TextDeserializer
    {
        public static Example ParseExample(this Model m, string line)
        {
            // An example line: 
            // 1 |a 0:1 |b 2:2

            var ex = new Example { Namespaces = new Dictionary<ushort, List<Feature>>() };

            // get namespaces, skip label
            foreach (var ns in line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                var fields = ns.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); // split features

                ushort featureGroup;
                UInt64 namespaceHash;
                m.ParseNamespace(fields[0], out featureGroup, out namespaceHash);

                ex.Namespaces.Add(featureGroup,
                    fields.Skip(1)
                        .Select(pairs => pairs.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries))
                            // split 0:1
                            .Select(s =>
                                new Feature
                                {
                                    WeightIndex = m.ParseFeature(s[0], namespaceHash),
                                    X = float.Parse(s[1], NumberStyles.Any, CultureInfo.InvariantCulture)
                                })
                            .ToList());
            }

            return ex;
        }
    }
}
