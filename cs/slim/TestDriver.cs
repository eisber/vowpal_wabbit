using System;
using System.IO;
using System.Linq;

namespace VowpalWabbit.Prediction
{
    enum Features
    {
        // Shared
        Modality_Audio = 0,
        Modality_Video = 1,
        CallType_P2P = 2,
        CallType_Server = 3,
        NetworkType_Wired = 4,
        NetworkType_Wifi = 5,
        // ADF
        Action1 = 6,
        Action2 = 7,
        Action3 = 8
    };

    enum Namespaces
    {
        SharedA = 0,
        SharedB = 1,
        SharedC = 2,
        ActionDependentX = 3,
        ActionDependentY = 4,
    };

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // using(var modelStream = File.OpenRead(@"C:\work\vw.csslim\vowpalwabbitslim\data\cb_data_5.model"))
                using(var modelStream = File.OpenRead(@"C:\work\vw.csslim\vowpalwabbitslim\data\regression_data_1.model"))
                {
                    Model m = ModelParser.Parse(modelStream);
                    var predictor = new VowpalWabbitPredictor(m);

                    Console.Out.WriteLine("Load model");

                    var ex1 = new Example();
            		// 1 |0 0:1
                    ex1.AddFeature(0, new Feature { WeightIndex = 0, X = 1 });

		            // 1 |0 0:5
                    var ex2 = new Example();
                    ex2.AddFeature(0, new Feature { WeightIndex = 0, X = 5 });

                    var preds = File.ReadLines(@"C:\work\vw.csslim\vowpalwabbitslim\data\regression_data_1.pred")
                        .Select(float.Parse).ToArray();

                    foreach (var p in preds)
                        Console.Out.WriteLine(p);

                    Console.Out.WriteLine(predictor.Predict(ex1));
                    Console.Out.WriteLine(predictor.Predict(ex2));
                }
            }
            catch (System.Exception e)
            {
                Console.Out.WriteLine(e.Message);
                Console.Out.WriteLine(e.StackTrace);
            }

            /*
            var model = ModelParser.ParseJson(File.ReadAllText(args[0]));

            var ex = new ContextualBanditExample
            {
                Shared = new Example()
            };

            ex.Shared.AddNamespace((ushort)Namespaces.SharedA, new[] { new Feature { WeightIndex = (ulong)Features.Modality_Audio, X = 1 } });
            ex.Shared.AddNamespace((ushort)Namespaces.SharedB, new[] { new Feature { WeightIndex = (ulong)Features.CallType_P2P, X = 1 } });
            ex.Shared.AddNamespace((ushort)Namespaces.SharedC, new[] { new Feature { WeightIndex = (ulong)Features.NetworkType_Wired, X = 1 } });

            var action1 = new Example();
            var action2 = new Example();
            var action3 = new Example();
            action1.AddNamespace((ushort)Namespaces.ActionDependentX, new[] { new Feature { WeightIndex = (ulong)Features.Action1, X = 1 } });
            action2.AddNamespace((ushort)Namespaces.ActionDependentX, new[] { new Feature { WeightIndex = (ulong)Features.Action2, X = 1 } });
            action3.AddNamespace((ushort)Namespaces.ActionDependentX, new[] { new Feature { WeightIndex = (ulong)Features.Action3, X = 1 } });

            ex.Actions.Add(action1);
            ex.Actions.Add(action2);
            ex.Actions.Add(action3);

            var scores = ex.Predict(model);

            Console.WriteLine(string.Join(" ", scores));
             */
        }
    }
}