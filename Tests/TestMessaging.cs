using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EmergoEntertainment.Messaging;
using System;

namespace EmergoEntertainment.Tests
{
    public class TestMessaging
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestCreation()
        {
            // Use the Assert class to test conditions
            try
            {
                new MessageDistributor();
            }
            catch (System.Exception)
            {
                Assert.Fail("Message Hub construction failed");
            }
        }

        [Test]
        public void TestMessage()
        {
            MessageDistributor messageDistributor = new MessageDistributor();
            try
            {
                MessageHub.StartListening("TestMessage", TestAction);
            }
            catch (System.Exception e)
            {
                Assert.Fail("Fail in message listen \n" + e.Message);
            }

            try
            {
                MessageHub.Enqueue(new Message("TestMessage", "Test", 42));
            }
            catch (System.Exception e)
            {
                Assert.Fail("Fail in message push \n" + e.Message);
            }

            try
            {
                Assert.True(messageDistributor.DequeueMessage());
            }
            catch (System.Exception e)
            {
                Assert.Fail("Fail in message dequeue with message in queue \n" + e.Message);
            }

            try
            {
                Assert.False(messageDistributor.DequeueMessage());
            }
            catch (System.Exception e)
            {
                Assert.Fail("Fail in message dequeue without message in queue \n" + e.Message);
            }
        }
        [Test]
        public void TestMessageUnsubscribe()
        {
            MessageDistributor dist = new MessageDistributor();
            try
            {
                MessageHub.StartListening("Test", TestAction);
                
                MessageHub.StopListening("Test", TestAction);
            }
            catch (System.Exception e)
            {
                Assert.Fail("Fail in message dequeue without message in queue \n" + e.Message);
            }
        }


        void TestAction(Message m)
        {
            Assert.AreEqual(m.data.Length, 2);
            Assert.AreSame(m.data[0], "Test");
            Assert.AreEqual(m.data[1], 42);
        }

        
        [UnityTest]
        public IEnumerator TestMessagingGetData()
        {
            MessageDistributor distributor = new MessageDistributor();

            MessageHub.StartListening("TestDesync", TestDesync);
            yield return null;
            MessageHub.Enqueue(new Message("TestDesync", "test"));
            yield return null;
            Assert.IsTrue(distributor.DequeueMessage());
        }

        private void TestDesync(Message obj)
        {
            try
            {
                Assert.IsTrue(obj.GetData<string>(out string s));
                Assert.AreEqual(s, "test");
            }
            catch (System.Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [UnityTest]
        public IEnumerator TestMessagingDirectUnsubscribe()
        {
            MessageDistributor distributor = new MessageDistributor();

            MessageHub.StartListening("TestDesync", TestDesyncUnsub);
            yield return null;
            MessageHub.Enqueue(new Message("TestDesync", "test", distributor));
            yield return null;
            Assert.IsTrue(distributor.DequeueMessage());
        }

        private void TestDesyncUnsub(Message obj)
        {
            try
            {
                Assert.IsTrue(obj.GetData<MessageDistributor>(out MessageDistributor dist));
                dist.messageHub._StopListening("TestDesync", TestDesyncUnsub);
            }
            catch (System.Exception e)
            {
                Assert.Fail(e.Message);

            }
        }
    }

    class MessageDistributor : IMessageDistributor
    {
        public MessageHub messageHub { get; set; }

        public MessageDistributor()
        {
            MessageHub.Create(this);
        }

        public bool DequeueMessage()
        {
            return messageHub._DequeueAndInvoke();
        }
    }
}
