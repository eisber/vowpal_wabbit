using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VowpalWabbit.Prediction.Tests
{
    [TestClass]
    public class OptionParserTests
    {
        [TestMethod]
        public void OptionParser_Test1()
        {
            CollectionAssert.AreEqual(new[] { "abc" }, OptionParser.FindOptions("-q abc", "-q").ToArray());
            CollectionAssert.AreEqual(new[] { "a", "b" }, OptionParser.FindOptions("-q a -q b", "-q").ToArray());
            CollectionAssert.AreEqual(new[] { "a", "b" }, OptionParser.FindOptions("-q a -d   -q  b", "-q").ToArray());
            CollectionAssert.AreEqual(new[] { "a", "b", "abc" }, OptionParser.FindOptions("-q a -d -q  b -q -q abc", "-q").ToArray());
        }

        [TestMethod]
        public void OptionParser_Empty()
        {
            Assert.AreEqual(0, OptionParser.FindOptions("-a b -qd ", "-q").Count());
            Assert.AreEqual(0, OptionParser.FindOptions("", "-q").Count());
            Assert.AreEqual(0, OptionParser.FindOptions("-a", "-q").Count());
            Assert.AreEqual(0, OptionParser.FindOptions("-q", "-q").Count());
            Assert.AreEqual(0, OptionParser.FindOptions("", "-q").Count());
            Assert.AreEqual(0, OptionParser.FindOptions("-q -d", "-q").Count());
        }

        [TestMethod]
        public void OptionParser_Float()
        {
            CollectionAssert.AreEqual(new[] { 0.5f }, OptionParser.FindOptionsFloat("--epsilon 0.5", "--epsilon").ToArray());
            CollectionAssert.AreEqual(new[] { -2f }, OptionParser.FindOptionsFloat("--lambda -2", "--lambda").ToArray());
            Assert.AreEqual(0, OptionParser.FindOptionsFloat("--epsilon", "--epsilon").Count());
            CollectionAssert.AreEqual(new[] { 0.5f, 0.4f }, OptionParser.FindOptionsFloat("--epsilon 0.5 --epsilon 0.4", "--epsilon").ToArray());
        }

        [TestMethod]
        public void OptionParser_Int()
        {
            CollectionAssert.AreEqual(new[] { 2 }, OptionParser.FindOptionsInt("--bag 2", "--bag").ToArray());
            Assert.AreEqual(0, OptionParser.FindOptionsInt("--bag", "--bag").Count());
            CollectionAssert.AreEqual(new[] { 2, 3 }, OptionParser.FindOptionsInt("--bag 2 --bag 3", "--bag").ToArray());
        }
    }
}
