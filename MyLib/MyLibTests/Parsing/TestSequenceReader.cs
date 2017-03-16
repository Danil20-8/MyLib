using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DRLib.Parsing;

namespace MyLibTests.Parsing
{
    [TestClass]
    public class TestSequenceReader
    {
        [TestMethod]
        public void TestSequenceReaderNext()
        {
            SequenceReader<char> sr = new SequenceReader<char>("hello/");
            sr.SetKeys(new char[] { '/' });
            sr.Next();
            Assert.AreEqual("hello", new string(sr.Result.ToArray()));
        }
    }
}
