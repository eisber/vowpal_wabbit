using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VowpalWabbit.Prediction
{
    //public class Namespace
    //{
    //    public ushort FeatureGroup { get; set; }

    //    public int Hash { get; set; }

    //    public IEnumerable<Feature> Features { get; set; }
    //}

    public class Example
    {
        // public List<Feature>[] Namespaces = new List<Feature>[256];
        // public HashSet<ushort> NamespacesAvailable = new HashSet<ushort>();
        // public List<Namespace> Namespaces = new List<Namespace>();
        public Dictionary<ushort, List<Feature>> Namespaces { get; set; }

        //public void AddFeatures(ushort featureGroup, List<Feature> features)
        //{
        //    if (this.Namespaces.TryGetValue(featureGroup, out var existingFeatures))
        //        existingFeatures.AddRange(features);
        //    else
        //        this.Namespaces.Add(featureGroup, features);

        //    //if (this.Namespaces[featureGroup] == null)
        //    //    this.Namespaces[featureGroup] = features.ToList();
        //    //else   
        //    //    this.Namespaces[featureGroup].AddRange(features);
        //}

        //public void AddFeature(ushort featureGroup, Feature feature)
        //{
        //    if (this.Namespaces[featureGroup] == null)
        //        this.Namespaces[featureGroup] = new List<Feature> { feature };
        //    else   
        //        this.Namespaces[featureGroup].Add(feature);
        //}

        //public void RemoveNamespace(ushort featureGroup)
        //{
        //    if (this.Namespaces.TryGetValue(featureGroup, out var features))
        //        features.Clear();
        //}

        //public void RemoveNamespace(ushort featureGroup)
        //{
        //    this.Namespaces[featureGroup]?.Clear();
        //}

        // public Example Merge(Example other)
        // {
        //     if (other == null)
        //         return this;

        //     var newEx = new Example { Namespaces = new Dictionary<ushort, List<Feature>>(this.Namespaces) };
        //     foreach (var ns in other.Namespaces)
        //         newEx.AddNamespace(ns.Key, ns.Value);
        //     return newEx;
        // }


    }
/*
    public class ExampleGuard : IDisposable
    {
        private readonly Example ex;
        
        private readonly ushort ns;

        public ExampleGuard(Example ex, ushort ns)
        {
            this.ex = ex;
            this.ns = ns;
        }

        public void Dispose()
        {
            
        }
    }
    */
}