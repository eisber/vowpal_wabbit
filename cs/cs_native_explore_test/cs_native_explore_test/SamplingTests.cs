using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace cs_native_explore_test
{
    [TestClass]
    public class SamplingTests
    {
        [TestMethod]
        public void Sampling_ChosingIndex()
        {
            var pdf = Microsoft.DecisionService.Exploration.ExplorationStrategies.GenerateEpsilonGreedy(0.5f, 3, 5);
            var freq = new int[pdf.Length];
            Console.WriteLine(string.Join(" ", pdf));

            for (int i = 0; i < 10000; i++)
            {
                string seed = "abc" + i;

                var nativeIndex = VowpalWabbit.Native.Exploration.Sampling.SampleAfterNormalizing(seed, pdf);
                var managedIndex = Microsoft.DecisionService.Exploration.Sampling.SampleAfterNormalizing(seed, pdf);

                Assert.AreEqual(nativeIndex, (UInt32)managedIndex);
                freq[nativeIndex]++;
            }

            var density = freq.Select(f => (float)f / freq.Sum()).ToArray();

            Console.WriteLine(string.Join(" ", freq));
            Console.WriteLine(string.Join(" ", density));
        }
    }
}
