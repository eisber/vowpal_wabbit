using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DecisionService.Prediction
{
    public class Example
    {
        public Dictionary<ushort, List<Feature>> Namespaces = new Dictionary<ushort, List<Feature>>();

        public void AddNamespace(ushort featureGroup, IEnumerable<Feature> features)
        {
            List<Feature> existingFeatures;
            if (this.Namespaces.TryGetValue(featureGroup, out existingFeatures))
                existingFeatures.AddRange(features);
            else
                this.Namespaces.Add(featureGroup, features.ToList());
        }

        public Example Merge(Example other)
        {
            if (other == null)
                return this;

            var newEx = new Example { Namespaces = new Dictionary<ushort, List<Feature>>(this.Namespaces) };
            foreach (var ns in other.Namespaces)
                newEx.AddNamespace(ns.Key, ns.Value);
            return newEx;
        }

        public string ToJson()
        {
            var json = new StringBuilder();
            json.Append('{')
                .Append(string.Join(",",
                   this.Namespaces.Select(kv =>
                    string.Format("\"{0}\":{1}",
                        kv.Key, string.Join(",", kv.Value.Select(f => f.ToJson()))))))
                .Append('}');

            return json.ToString();
        }
    }
}