using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using VowpalWabbit.Prediction;

namespace VowpalWabbit.Prediction.Tests
{
    [TestClass]
    public class ContextualBanditTests
    {
        public TestContext TestContext { get; set; }

        private void PrintMatrix(string message, float[,] m)
        {
            Console.WriteLine(message);

            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    Console.Write(m[i, j]);
                    Console.Write(" ");
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        private void ContextualBandit(string modelFile, int repetitions, float[] expectedPDF, float[,] expectedHistogram)
        {
            var dataDir = Path.Combine(
                TestContext.TestDir,
                @"..\..\..\..\vowpalwabbitslim\data");

            using (var modelStream = File.OpenRead(Path.Combine(dataDir, modelFile)))
            {
                Model m = ModelParser.Parse(modelStream);
                var predictor = VowpalWabbitPredictor.Create(m) as VowpalWabbitPredictorContextualBandit;

                var examples = File.ReadAllLines(Path.Combine(dataDir, "cb_data_5.txt"))
                    .Take(4)
                    .Select(TextDeserializer.ParseExample)
                    .ToList();

                var actions = new MultilineExample
                {
                    Shared = examples[0],
                    Examples = examples.Skip(1).ToList()
                };

                var actualHistogram = new float[3, 3];

                for (int i = 0; i < repetitions; i++)
                {
                    var eventId = "abcde" + i;

                    var actual = predictor.Predict(eventId, actions);

                    if (i == 0)
                    {
                        Console.WriteLine("Actual:   " + string.Join(" ", actual.Probabilities));
                        Console.WriteLine("Expected: " + string.Join(" ", expectedPDF));

                        CollectionAssert.AreEqual(expectedPDF, actual.Probabilities, new FuzzyFloatComparer(0.001));
                    }

                    for (int j = 0; j < actual.Indicies.Length; j++)
                        actualHistogram[actual.Indicies[j], j]++;
                }

                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        actualHistogram[i, j] /= repetitions;

                PrintMatrix("Actual", actualHistogram);
                PrintMatrix("Expected", expectedHistogram);

                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        Assert.AreEqual(expectedHistogram[i, j], actualHistogram[i, j], 1e-2f);
            }
        }

        [TestMethod]
        public void ContextualBanditTest_5()
        {
            ContextualBandit("cb_data_5.model", 10000,
                expectedPDF: new[] { 0.1f, 0.1f, 0.8f },
                expectedHistogram: new float[3, 3] {
                    // see top action 2 w / 0.8
                    {0.1f, 0.1f, 0.8f }, // slot 0
                    // most of the time we should see action 1 (which is ranked 2nd)
                    // in 10% we should see the top action 2 swapped from top-slot to here
                    {0.1f, 0.9f, 0.0f }, // slot 1
                    // most of the time we should see action 0 (which is ranked 3rd)
                    // in 20% we should see the top action 2 swapped from top-slot to here
                    {0.8f, 0.0f, 0.2f }  // slot 2
                    });
        }

        [TestMethod]
        public void ContextualBanditTest_6()
        {
            ContextualBandit("cb_data_6.model", 1000,
                expectedPDF: new[] { 0.329f, 0.333f, 0.337f },
                expectedHistogram: new float[3, 3] {
                    {0.311f, 0.352f, 0.337f}, // slot 0
                    {0.352f, 0.648f, 0.0f }, // slot 1
                    {0.337f, 0.0f, 0.663f }  // slot 2
                    });
        }

        [TestMethod]
        public void ContextualBanditTest_7()
        {
            ContextualBandit("cb_data_7.model", 5,
                expectedPDF: new[] { 0.0f, 0.0f, 1.0f },
                expectedHistogram: new float[3, 3] {
                    {0.0f, 0.0f, 1.0f}, // slot 0
                    {0.0f, 1.0f, 0.0f}, // slot 1
                    {1.0f, 0.0f, 0.0f}  // slot 2
                    });
        }

        [TestMethod]
        public void ContextualBanditTest_8()
        {
            ContextualBandit("cb_data_8.model", 10000,
                expectedPDF: new[] { 0.09f, 0.09f, 0.82f },
                expectedHistogram: new float[3, 3] {
                    {0.09f, 0.09f, 0.82f}, // slot 0
                    {0.09f, 0.91f, 0.00f}, // slot 1
                    {0.82f, 0.00f, 0.18f}  // slot 2
                    });
        }
    }
}
