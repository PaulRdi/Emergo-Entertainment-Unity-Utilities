using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EmergoEntertainment.Messaging;

namespace EmergoEntertainment.UnityUtilities.Tests
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
                MessageHub.StartListening(MessageType.ExampleMessageType, TestAction);
            }
            catch (System.Exception e)
            {
                Assert.Fail("Fail in message listen \n" + e.Message);
            }

            try
            {
                MessageHub.Enqueue(new Message(MessageType.ExampleMessageType, "Test", 42));
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

        void TestAction(Message m)
        {
            Assert.AreEqual(m.data.Length, 2);
            Assert.AreSame(m.data[0], "Test");
            Assert.AreEqual(m.data[1], 42);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestMessagingWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
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
