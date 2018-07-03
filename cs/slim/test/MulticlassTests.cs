using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using VowpalWabbit.Prediction;

namespace VowpalWabbit.Prediction.Tests
{
    [TestClass]
    public class MulticlassTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Multiclass4()
        {
            var dataDir = Path.Combine(
                TestContext.TestDir,
                @"..\..\..\..\vowpalwabbitslim\data");

            using (var modelStream = File.OpenRead(Path.Combine(dataDir, "multiclass_data_4.model")))
            {
                Model m = ModelParser.Parse(modelStream);
                var predictor = new VowpalWabbitPredictorMulticlass(m);

                var examples = File.ReadAllLines(Path.Combine(dataDir, "multiclass_data_4.txt"))
                    .Where(l => l.Trim().Length > 0)
                    .Select(TextDeserializer.ParseExample)
                    .ToList();

                var actual = predictor.Predict(
                    new MultilineExample
                    {
                        Shared = examples[0],
                        Examples = examples.Skip(1).ToList()
                    });

                var expected = new[] { 0.901038f, 0.46983f, 0.0386223f };

                System.Console.WriteLine("Actual:   " + string.Join(" ", actual));
                System.Console.WriteLine("Expected: " + string.Join(" ", expected));

                CollectionAssert.AreEqual(expected, actual, new FuzzyFloatComparer(0.00001));
            }
        }
    }
}
