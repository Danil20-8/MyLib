using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyLib.Serialization;
using MyLib.Serialization.Binders;


namespace MyLibTests
{
    [TestClass]
    public class TestNullBinder
    {
        [TestMethod]
        public void TestNullBinderSerializationValue()
        {
            var tc = new TestClassWithNullField();
            tc.nullField = "Hello";
            string r = new CompactSerializer().Serialize(tc);
            Assert.AreEqual("1&Hello~&", r);
            Assert.AreEqual("Hello", new CompactSerializer().Deserialize<TestClassWithNullField>(r).nullField);
        }
        [TestMethod]
        public void TestNullBinderSerializationNull()
        {
            var tc = new TestClassWithNullField();
            tc.nullField = null;
            string r = new CompactSerializer().Serialize(tc);
            Assert.AreEqual("0&&", r);
            Assert.AreEqual(null, new CompactSerializer().Deserialize<TestClassWithNullField>(r).nullField);
        }
    }

    class TestClassWithNullField
    {

        [Addon]
        [SerializeBinder(typeof(NullBinder<string>))]
        public string nullField = null;
    }
}
