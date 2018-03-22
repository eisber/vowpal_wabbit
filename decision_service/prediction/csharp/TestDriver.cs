using System;
using System.IO;

namespace Microsoft.DecisionService.Prediction
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
            var model = ModelParser.Parse(File.ReadAllText(args[0]));

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
        }
    }
}