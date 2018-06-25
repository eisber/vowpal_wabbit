using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VowpalWabbit.Prediction;

namespace VowpalWabbit.JSON
{
    public static class JsonDeserializer
    {
        public static Example ParseExample(JObject json)
        {
            var ex = new Example { Namespaces = new Dictionary<ushort, List<Feature>>() };

            foreach (var ns in json)
            {
                if (ns.Key.StartsWith("_"))
                    continue;

                ushort featureGroup;
                UInt64 namespaceHash;
                HashUtil.ParseNamespace(ns.Key, out featureGroup, out namespaceHash);

                var features = new List<Feature>();
                foreach (var feature in ns.Value.OfType<JProperty>())
                {
                    var f = new AuditFeature
                    {
                        Name = feature.Name,
                        Namespace = ns.Key
                    };
                    features.Add(f);

                    switch (feature.Value.Type)
                    {
                        case JTokenType.Boolean:
                            if (feature.Value.Value<bool>())
                                f.X = 1f;
                            break;
                        case JTokenType.String:
                            f.Name += feature.Value.Value<string>();
                            f.X = 1f;
                            break;
                        case JTokenType.Integer:
                            f.X = (float)feature.Value.Value<long>();
                            break;
                        case JTokenType.Float:
                            f.X = (float)feature.Value.Value<double>();
                            break;
                        default:
                            throw new NotSupportedException("Unsupported feature type: " + feature.Value.Type + " of feature " + feature.Name);
                    }

                    f.WeightIndex = HashUtil.ParseFeature(f.Name, namespaceHash);
                }

                if (features.Count > 0)
                    ex.Namespaces.Add(featureGroup, features);
            }

            return ex;
        }

        public static MultilineExample ParseMultilineExample(JObject json)
        {
            return new MultilineExample
            {
                // parse shared features
                Shared = ParseExample(json),
                // parse actions
                Examples = ((JArray)json["_multi"])
                    .OfType<JObject>()
                    .Select(ParseExample)
                    .ToList()
            };
        }

        public static MultilineExample ParseDecisionServiceExample(string json)
        {
            var jobj = JObject.Parse(json);
            var context = jobj["c"] as JObject;
            if (context == null)
                return null;

            return ParseMultilineExample(context);
        }
    }
}
