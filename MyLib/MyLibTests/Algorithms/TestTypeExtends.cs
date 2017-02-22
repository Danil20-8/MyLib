using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyLib.Algoriphms;

namespace MyLibTests.Algorithms
{
    [TestClass]
    public class TestTypeExtends
    {
        [TestMethod]
        public void TestGetGeneration()
        {
            // int : ValueType, object
            // int is the third generation relatively object, so it has 2 parents
            Assert.AreEqual(2, typeof(int).GetGeneration(typeof(object)));

            // parent type int is the tested type.
            Assert.AreEqual(0, typeof(int).GetGeneration(typeof(int)));

            // float is not a base type of int
            Assert.AreEqual(-1, typeof(int).GetGeneration(typeof(float)));

        }
    }
}
