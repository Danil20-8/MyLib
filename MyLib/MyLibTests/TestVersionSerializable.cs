using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyLib.Serialization;
namespace MyLibTests
{
    [TestClass]
    public class TestVersionSerializable
    {
        [TestMethod]
        public void TestActualSerialize()
        {
            //using BClass
            string r = new CompactSerializer().Serialize(new MainClass());

            Assert.AreEqual("2&1,5&", r);

            Assert.AreEqual(1.5f, new CompactSerializer().Deserialize<MainClass>(r).value);
        }
        [TestMethod]
        public void TestNotActualSerialize()
        {
            //using AClass
            Assert.AreEqual(1, new CompactSerializer().Deserialize<MainClass>("1&1&").value);
        }
    }

    class MainClass
    {
        public float value = 1.5f;

        [Version(1)]
        class AClass : IBinder
        {
            [Addon]
            int value { get { return (int)c.value; } set { c.value = value; } }

            MainClass c;
            public AClass(MainClass c)
            {
                this.c = c;
            }

            public AClass()
            {
                c = new MainClass();
            }

            public object GetResult()
            {
                return c;
            }
        }

        [Version(2)]
        class BClass : IBinder
        {
            [Addon]
            float value { get { return c.value; } set { c.value = value; } }

            MainClass c;
            public BClass(MainClass c)
            {
                this.c = c;
            }

            public BClass()
            {
                c = new MainClass();
            }

            public object GetResult()
            {
                return c;
            }
        }
    }
}
