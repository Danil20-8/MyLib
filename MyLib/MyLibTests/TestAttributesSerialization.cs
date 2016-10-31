using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyLib.Serialization;
using MyLib.Serialization.Binders;
using System.Collections.Generic;
using MyLib.Serialization.Binders;
namespace MyLibTests
{
    [TestClass]
    public class TestAttributesSerialization
    {
        [TestMethod]
        public void TestSerializationBinder()
        {
            bar b = new bar();
            b.dict.Add("vasya", 12);

            CompactSerializer cs = new CompactSerializer();

            string d = cs.Serialize(b);
            cs.Deserialize<bar>(d);
        }
        [TestMethod]
        public void TestSerializationBinderInNumerables()
        {
            bar[] b = new bar[] { new bar(), new bar(), new bar() };
            b[2].dict.Add("vasya", 12);
            CompactSerializer cs = new CompactSerializer(CSOptions.WithTypes);

            string d = cs.Serialize(b);
            cs.Deserialize<bar[]>(d);
        }
        [TestMethod]
        public void TestEqualBindersOnType()
        {
            CompactSerializer cs = new CompactSerializer();
            Assert.AreEqual("a&", cs.Serialize(new classWithBinder1()));
            Assert.AreEqual("1&", cs.Serialize(new classWithBinder2()));
            Assert.AreEqual(1, cs.Deserialize<classWithBinder1>("a&").a);
        }
        [TestMethod]
        public void TestTypeBinder()
        {
            CompactSerializer cs = new CompactSerializer();
            Assert.AreEqual("binderedClass&", cs.Serialize(new BinderedClass()));
            Assert.AreEqual(1, cs.Deserialize<BinderedClass>("binderedClass&").a);
        }
        [TestMethod]
        public void TestDictionaryArrayBinder()
        {
            CompactSerializer cs = new CompactSerializer();
            string d = cs.Serialize(new Dictionary<int, int>[] { new Dictionary<int, int>() { { 1, 1 } }, new Dictionary<int, int>() { { 1, 1 } } }, typeof(DictionaryArrayBinder<int, int>));
            Assert.AreEqual("2&1&1&1&1&1&1&", d);
        }
    }

    public class bar
    {
        [Addon]
        [SerializeBinder(typeof(DictionaryBinder<string, int>))]
        public Dictionary<string, int> dict = new Dictionary<string, int>();
    }

    public class classWithBinder1
    {
        [Addon]
        [SerializeBinder(typeof(IntBinder))]
        public int a = 1;
    }
    public class classWithBinder2
    {
        [Addon]
        int a = 1;
    }
    [SerializeBinder(typeof(BinderedClassBinder))]
    public class BinderedClass
    {
        public int a = 1;
    }
    public struct BinderedClassBinder : IBinder
    {
        BinderedClass bc;
        [Addon]
        string v { get { return "binderedClass"; } set { bc = new BinderedClass(); } }
        public BinderedClassBinder(BinderedClass bc)
        {
            this.bc = bc;
        }
        public object GetResult()
        {
            return bc;
        }
    }
    public class IntBinder : IBinder
    {
        int a;
        [Addon]
        string s { get { return mul("a", a); } set { a = value.Length; } }
        public IntBinder(int a)
        {
            this.a = a;
        }
        public IntBinder()
        {

        }
        public object GetResult()
        {
            return a;
        }
        string mul(string str, int value)
        {
            string s = "";
            for (int i = 0; i < value; i++)
                s += str;
            return s;
        }
    }
}
