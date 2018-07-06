using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace cs_native_explore_test
{
    [TestClass]
    public class ExplorationStrategiesTests
    {
        [TestMethod]
        public void EpsilonGreedy()
        {
            float[] managed = Microsoft.DecisionService.Exploration.ExplorationStrategies.GenerateEpsilonGreedy(0.2f, 0, 4);
            float[] native = VowpalWabbit.Native.Exploration.ExplorationStrategies.GenerateEpsilonGreedy(0.2f, 0, 4);

            Console.WriteLine("managed: " + string.Join(",", managed));
            Console.WriteLine("native: " + string.Join(",", native));
            CollectionAssert.AreEqual(managed, native);
        }

        [TestMethod]
        public void Softmax()
        {
            var scores = new float[] { -2f, -1f, -0.5f };

            float[] managed = Microsoft.DecisionService.Exploration.ExplorationStrategies.GenerateSoftmax(-2f, scores);
            float[] native = VowpalWabbit.Native.Exploration.ExplorationStrategies.GenerateSoftmax(-2f, scores);

            Console.WriteLine("managed: " + string.Join(",", managed));
            Console.WriteLine("native: " + string.Join(",", native));
            CollectionAssert.AreEqual(managed, native);
        }

        [TestMethod]
        public void Bag()
        {
            var topActions = new int[] { 5, 2, 0 };

            float[] managed = Microsoft.DecisionService.Exploration.ExplorationStrategies.GenerateBag(topActions);
            float[] native = VowpalWabbit.Native.Exploration.ExplorationStrategies.GenerateBag(topActions);

            Console.WriteLine("managed: " + string.Join(",", managed));
            Console.WriteLine("native: " + string.Join(",", native));
            CollectionAssert.AreEqual(managed, native);
        }
    }
}
