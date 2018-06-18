using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VowpalWabbit.Prediction;

namespace test
{
    [TestClass]
    public class RegressionTests
    {
        public TestContext TestContext { get; set; }

        public ushort ParseFeatureGroup(Model m, string s)
        {
            m.ParseNamespace(s, out var featureGroup, out var _);

            return featureGroup;
        }

        public UInt64 ParseFeatureIndex(Model m, string ns, string featureIndex)
        {
            m.ParseNamespace(ns, out var _, out var namespaceHash);

            return m.ParseFeature(featureIndex, namespaceHash);
        }

        public void Regression(string modelFile, string predFile, string dataFile)
        {
            var dataDir = Path.Combine(
                TestContext.TestDir,
                @"..\..\..\..\vowpalwabbitslim\data");

            using (var modelStream = File.OpenRead(Path.Combine(dataDir, modelFile)))
            {
                Model m = ModelParser.Parse(modelStream);
                var predictor = new VowpalWabbitPredictor(m);

                var actual = new List<float>();
                // Parse data file
                foreach (var line in File.ReadAllLines(Path.Combine(dataDir, dataFile)))
                {
                    Example ex = m.ParseExample(line);

                    // help debugging
                    Console.Out.WriteLine("Input  " + line);
                    Console.Out.WriteLine("Parsed " + ex.ToText());
                    Console.Out.WriteLine("Parsed " + ex.ToJson());
                    Console.WriteLine();

                    actual.Add(predictor.Predict(ex));
                };

                // parse expected
                var expected = File.ReadLines(Path.Combine(dataDir, predFile))
                    .Select(float.Parse).ToArray();

                Console.Out.WriteLine("Actual:   " + string.Join(",", actual));
                Console.Out.WriteLine("Expected: " + string.Join(",", expected));

                CollectionAssert.AreEqual(expected, actual, new FuzzyFloatComparer(0.00001));
            }
        }

        [TestMethod]
        public void RegressionTest1()
        {
            Regression("regression_data_1.model", "regression_data_1.pred", "regression_data_1.txt");
        }

        [TestMethod]
        public void RegressionTest1_NoConstant()
        {
            Regression("regression_data_no_constant.model", "regression_data_no_constant.pred", "regression_data_1.txt");
        }

        [TestMethod]
        public void RegressionTest1_IgnoreLinear()
        {
            Regression("regression_data_ignore_linear.model", "regression_data_ignore_linear.pred", "regression_data_2.txt");
        }

        [TestMethod]
        public void RegressionTest2()
        {
            Regression("regression_data_2.model", "regression_data_2.pred", "regression_data_2.txt");
        }

        [TestMethod]
        public void RegressionTest3()
        {
            Regression("regression_data_3.model", "regression_data_3.pred", "regression_data_3.txt");
        }

        [TestMethod]
        public void RegressionTest4()
        {
            Regression("regression_data_4.model", "regression_data_4.pred", "regression_data_4.txt");
        }

        [TestMethod]
        public void RegressionTest5()
        {
            Regression("regression_data_5.model", "regression_data_5.pred", "regression_data_4.txt");
        }

        [TestMethod]
        public void RegressionTest6()
        {
            Regression("regression_data_6.model", "regression_data_6.pred", "regression_data_3.txt");
        }

        [TestMethod]
        public void RegressionTest7()
        {
            Regression("regression_data_7.model", "regression_data_7.pred", "regression_data_7.txt");
        }
    }
}
