using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyLib.Serialization;
namespace MyLibTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestSimpleDataSerialization()
        {
            int a = 10;
            CompactSerializer cs = new CompactSerializer();
            string d = cs.Serialize(a);
            Assert.AreEqual("10&", d);
            a = cs.Deserialize<int>(d);
            Assert.AreEqual(10, a);
        }
        [TestMethod]
        public void TestStringSerialization()
        {
            string s = "qwerty";
            CompactSerializer cs = new CompactSerializer();
            string d = cs.Serialize(s);
            Assert.AreEqual("qwerty&", d);
            Assert.AreEqual("qwerty", cs.Deserialize<string>(d));
        }
        [TestMethod]
        public void TestStringNumerables()
        {
            int[] a = new int[] { 1, 2 };
            CompactSerializer cs = new CompactSerializer();
            string d = cs.Serialize(a);
            Assert.AreEqual("2&1&2&", d);
            Assert.AreEqual(2, cs.Deserialize<int[]>(d)[1]);
        }
        [TestMethod]
        public void TestStringNumerablesObject()
        {
            object[] a = new object[] { 1.2, 2 };
            CompactSerializer cs = new CompactSerializer();
            string d = cs.Serialize(a);
            //Assert.AreEqual(":2&System.Double:1,2&System.Int32:2&", d);
            Assert.AreEqual(2, (int) ((cs.Deserialize<object[]>(d))[1]));
        }
        [TestMethod]
        public void TestAbstract()
        {
            abs[] a = new abs[] { new sba(13), new sba(24), new sba(48) };
            CompactSerializer cs = new CompactSerializer();
            string d = cs.Serialize(a);
            Assert.AreEqual(48, ((sba) cs.Deserialize<abs[]>(d)[2]).a);
        }
        [TestMethod]
        public void TestAbstractInShell()
        {
            ShellAbstract[] a = new ShellAbstract[] { new ShellAbstract() };
            CompactSerializer cs = new CompactSerializer();
            string r = cs.Serialize(a);
            Assert.AreEqual(1, cs.Deserialize<ShellAbstract[]>(r)[0].field.a);
        }
        [TestMethod]
        public void TestEmptyClass()
        {
            empty[] a = new empty[] { new empty(), new empty(), new empty() };
            CompactSerializer cs = new CompactSerializer();
            string d = cs.Serialize(a);
            Assert.AreEqual(3, cs.Deserialize<empty[]>(d).Length);
        }
        [TestMethod]
        public void TestSerializeable()
        {
            sable[] a = new sable[] { new sable(), new sable(), new sable() };
            CompactSerializer cs = new CompactSerializer();
            string d = cs.Serialize(a);
            Assert.AreEqual(1, cs.Deserialize<sable[]>(d)[2].a);
        }
        [TestMethod]
        public void TestSafeStrings()
        {
            SafeStrings ss = new SafeStrings() { s1 = "111&", s2 = "222&" };
            CompactSerializer cs = new CompactSerializer(CSOptions.SafeString);
            string d = cs.Serialize(ss);
            Assert.AreEqual("111&222&", cs.Deserialize<SafeStrings>(d).ToString());
        }
        [TestMethod]
        public void TestMultiLine()
        {
            SafeStrings ss = new SafeStrings() { s1 = "111", s2 = "222" };
            CompactSerializer cs = new CompactSerializer('\n');
            string d = cs.Serialize(ss);
            Assert.AreEqual("111222", cs.Deserialize<SafeStrings>(d).ToString());
        }

        [TestMethod]
        public void TestTypesCollecting()
        {
            Dynamic d = new Dynamic();
            var t = new MyLib.TreeNode<int>(1);
            t.AddNode(3);
            d.value = t;
            var r = CompactSerializer.CollectDynamicTypes(d);
            Assert.IsTrue(true);
        }
    }
    public abstract class abs
    {
        [ConstructorArg(0)]
        public int a;
        [Addon]
        public empty e = new empty();
        public abs(int a)
        {
            this.a = a;
        }
    }
    public class sba : abs
    {
        public sba(int a): base(a)
        {

        }
    }
    public class ShellAbstract
    {
        [Addon]
        public abs field = new sba(1);
    }
    [Serializable]
    public class sable
    {
        public int a = 1;
        public double d = 2.2;
        public string s = "sss";
    }
    public class empty
    {

    }
    [Serializable]
    public class SafeStrings
    {
        public string s1;
        public string s2;
        public override string ToString()
        {
            return s1 + s2;
        }
    }

    [Serializable]
    class Dynamic
    {
        public object value;
    }
}
