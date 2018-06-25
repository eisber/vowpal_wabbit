using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VowpalWabbit.JSON;
using VowpalWabbit.Prediction;

namespace VowpalWabbit.Diagnostics
{
    class Program
    {
        static Dictionary<ulong, AuditFeature> DiscoverIndicies(Model m, string dataFile)
        {
            var sparse = (SparseModelWeights)m.Weights;
            var missingWeightIndicies = new HashSet<UInt64>(sparse.Weights.Keys);

            var discoveredFeatures = new Dictionary<ulong, AuditFeature>();

            using (var dataInput = new StreamReader(File.OpenRead(dataFile), Encoding.UTF8))
            {
                string line;
                int count = 0;
                while ((line = dataInput.ReadLine()) != null)
                {
                    var ex = JsonDeserializer.ParseDecisionServiceExample(line);

                    if (ex == null)
                        continue;

                    foreach (var fullFeature in ex.Audit(m))
                    {
                        if (missingWeightIndicies.Contains(fullFeature.WeightIndex))
                        {
                            discoveredFeatures.Add(fullFeature.WeightIndex, fullFeature);
                            missingWeightIndicies.Remove(fullFeature.WeightIndex);
                        }

                        if (missingWeightIndicies.Count == 0)
                            return discoveredFeatures;
                    }

                    if (count++ > 10000)
                    {
                        Console.WriteLine("Unable to find all features in the first 10000 lines");
                        return discoveredFeatures;
                    }
                }
            }

            return discoveredFeatures;
        }

        static void Main(string[] args)
        {
            using (var input = File.OpenRead(args[0]))
            using (var output = new StreamWriter(File.OpenWrite(args[0] + ".readable")))
            {
                var m = ModelParser.Parse(input);
                var sparse = (SparseModelWeights)m.Weights;

                if (args.Length > 1)
                {
                    Dictionary<ulong, AuditFeature> discoveredFeatures = DiscoverIndicies(m, args[1]);

                    foreach (var kv in sparse.Weights)
                    {
                        output.Write(kv.Key + " " + kv.Value);

                        if (discoveredFeatures.TryGetValue(kv.Key, out var feature))
                            output.Write(" " + feature.Namespace + " " + feature.Name);

                        output.WriteLine();
                    }

                    return;
                }

                // write weights
                foreach (var kv in sparse.Weights)
                    output.WriteLine(kv.Key + " " + kv.Value);
            }
        }
    }
}
