using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DRLib.Patterns.Observer;

namespace MyLibTests.Patterns.Observer
{
    [TestClass]
    public class TestMessenger
    {
        string sender = "Sender";
        string message1 = "I am object";
        string message2 = "I am string";

        [TestMethod]
        public void TestAddListenerAndSendMessage()
        {
            Messenger messenger = new Messenger();

            bool done = false;

            Action<object, string> assertAction = (s, m) =>
            {
                Assert.AreEqual(sender, s);
                Assert.AreEqual(message1, m);

                done = true;
            };

            messenger.AddListener(assertAction);

            messenger.Send((object)sender, message1);
            messenger.Send("BadSender", "Bad message");

            Assert.IsTrue(done);
        }

        [TestMethod]
        public void TestMessengerDifferntTypesOfOneSender()
        {
            Messenger messenger = new Messenger();

            messenger.AddListener<object, string>(AssertObject);
            messenger.AddListener<string, string>(AssertString);

            messenger.Send((object)sender, message1);
            messenger.Send(sender, message2);
        }
        [TestMethod]
        public void TestSendToVoid()
        {
            Messenger messenger = new Messenger();

            messenger.Send(sender, message1);
        }
        [TestMethod]
        public void TestSendToRemoved()
        {
            Messenger messenger = new Messenger();

            Action<string, string> assertFail = (s, m) =>
            {
                Assert.Fail();
            };

            messenger.AddListener(assertFail);
            messenger.RemoveListener(assertFail);

            messenger.Send(sender, message1);
        }

        void AssertObject(object s, string m)
        {
            Assert.AreEqual(sender, s);
            Assert.AreEqual(message1, m);
        }
        void AssertString(string s, string m)
        {
            Assert.AreEqual(sender, s);
            Assert.AreEqual(message2, m);
        }
    }
}
