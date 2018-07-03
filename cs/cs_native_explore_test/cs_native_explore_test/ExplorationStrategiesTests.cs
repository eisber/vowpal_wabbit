using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            CollectionAssert.AreEqual(managed, native);
        }
    }
}
