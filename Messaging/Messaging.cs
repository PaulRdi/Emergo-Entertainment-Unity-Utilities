using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergoEntertainment.Messaging
{
    
    /// <summary>
    /// The base class for the messaging system.
    /// Access via GameManager.instance.messageHub or use static methods to interact.
    /// </summary>
    public class MessageHub
    {
        /// <summary>
        /// The reference to the object where the MessageHub is saved (e.g. a GameManager)
        /// </summary>
        static IMessageDistributor _distributor;
        /// <summary>
        /// The queue of messages to be sent. Only one message can be sent at a time.
        /// </summary>
        public readonly Queue<Message> messageQueue;
        public readonly Queue<Message> messageReturnQueue;
        /// <summary>
        /// A dictionarly of actions mapped to message types. One message Type has multiple actions associated.
        /// </summary>
        Dictionary<string, List<MessageAction>> messageDict;
        Dictionary<string, List<MessageAction>> messageDictSyncList;
        Dictionary<string, List<MessageAction>> messageDictAddList;

        Dictionary<string, List<MessageAction<MessageReturnType>>> messageReturnDict;
        Dictionary<string, List<MessageAction<MessageReturnType>>> messageReturnDictSyncList;
        Dictionary<string, List<MessageAction<MessageReturnType>>> messageReturnDictAddList;

        Dictionary<string, Queue<Action<MessageReturnType>>> returningMessageFunctionPointers;

        private MessageHub()
        {
            messageQueue = new Queue<Message>();
            messageReturnQueue = new Queue<Message>();
            messageDict = new Dictionary<string, List<MessageAction>>();
            messageDictSyncList = new Dictionary<string, List<MessageAction>>();
            messageDictAddList = new Dictionary<string, List<MessageAction>>();
            messageReturnDict = new Dictionary<string, List<MessageAction<MessageReturnType>>>();
            messageReturnDictSyncList = new Dictionary<string, List<MessageAction<MessageReturnType>>>();
            messageReturnDictAddList = new Dictionary<string, List<MessageAction<MessageReturnType>>>();
            returningMessageFunctionPointers = new Dictionary<string, Queue<Action<MessageReturnType>>>();
        }

        public static void Create(IMessageDistributor distributor)
        {
            MessageHub instance = new MessageHub();
            distributor.messageHub = instance;
            _distributor = distributor;
        }

        /// <summary>
        /// Subscribes an action to a string. The action gets invoked everytime the messageQueue is popped.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        public void _StartListening(string msg, Action<Message> a, int p = 10, bool persist = false)
        {
            MessageAction messageAction = new MessageAction(a, p, persist);
            if (messageDict.ContainsKey(msg) && messageDict[msg].Contains(messageAction))
                throw new MessagingException("Messages of type: " + msg + " are allready executing action " + a.Method.ToString());

            if (messageDictAddList.ContainsKey(msg))
            {
                MessageAction validateAction = messageDictAddList[msg].FirstOrDefault(msga => msga.action == a);
                if (validateAction != default(MessageAction))
                    throw new MessagingException("You are trying to add the same action to the same message type twice. MessageType: " + msg);

                messageDictAddList[msg].Add(messageAction);
            }
            else
            {
                messageDictAddList.Add(msg, new List<MessageAction>());
                messageDictAddList[msg].Add(messageAction);
            }
        }

        /// <summary>
        /// Subscribes an action to a string. The action gets invoked everytime the messageQueue is popped.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        public void _StartListening(string msg, Func<Message, MessageReturnType> f, MethodPriority p = MethodPriority.Default, bool persist = false)
        {
            MessageAction<MessageReturnType> messageAction = new MessageAction<MessageReturnType>(f, p, persist);
            if (messageReturnDict.ContainsKey(msg) && messageReturnDict[msg].Contains(messageAction))
                throw new MessagingException("Messages of type: " + msg + " are allready executing action " + f.Method.ToString());

            if (messageReturnDictAddList.ContainsKey(msg))
            {
                MessageAction<MessageReturnType> validateAction = messageReturnDictAddList[msg].FirstOrDefault(msga => msga.action == f);
                if (validateAction != default(MessageAction<MessageReturnType>))
                    throw new MessagingException("You are trying to add the same action to the same message type twice.");

                messageReturnDictAddList[msg].Add(messageAction);
            }
            else
            {
                messageReturnDictAddList.Add(msg, new List<MessageAction<MessageReturnType>>());
                messageReturnDictAddList[msg].Add(messageAction);
            }
        }
        /// <summary>
        /// Call to remove an action from given message type.
        /// Only removes once the message queue is popped.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        public void _StopListening(string msg, Action<Message> a)
        {
            UpdateListeners();
            if (messageDict.ContainsKey(msg))
            {
                MessageAction messageActionInMessageDict = messageDict[msg].FirstOrDefault(m => m.action == a);
                if (messageActionInMessageDict == default(MessageAction))
                {
                    throw new MessagingException("You are trying to remove " + a.ToString() + " from message " + msg + " but there is no such action assigned to this message");
                }
                if (messageDictSyncList.ContainsKey(msg))
                {
                    MessageAction messageActionInSyncList = messageDictSyncList[msg].FirstOrDefault(m => m.action == a);
                    if (messageActionInSyncList != default(MessageAction))
                    {
                        throw new MessagingException("Messages of type: " + msg + " are allready removing action " + a.Method.ToString());
                    }
                    messageDictSyncList[msg].Add(messageDict[msg].First(m => m.action == a));
                }
                else
                {
                    messageDictSyncList.Add(msg, new List<MessageAction>());
                    messageDictSyncList[msg].Add(messageDict[msg].First(m => m.action == a));
                }
            }
            else
                throw new MessagingException("You are trying to remove an action from message type " + msg + ". But noting is listening this message type.");
        }
        /// <summary>
        /// Call to remove an action from given message type.
        /// Only removes once the message queue is popped.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        public void _StopListening(string msg, Func<Message, MessageReturnType> f)
        {
            UpdateReturnListeners();
            if (messageReturnDict.ContainsKey(msg))
            {
                MessageAction<MessageReturnType> messageActionInMessageDict = messageReturnDict[msg].FirstOrDefault(m => m.action == f);
                if (messageActionInMessageDict == default(MessageAction<MessageReturnType>))
                {
                    throw new MessagingException("You are trying to remove " + f.ToString() + " from message " + msg + " but there is no such action assigned to this message");
                }
                if (messageReturnDictSyncList.ContainsKey(msg))
                {
                    MessageAction<MessageReturnType> messageActionInSyncList = messageReturnDictSyncList[msg].FirstOrDefault(m => m.action == f);
                    if (messageActionInSyncList != default(MessageAction<MessageReturnType>))
                    {
                        throw new MessagingException("Messages of type: " + msg + " are allready removing action " + f.Method.ToString());
                    }
                    messageReturnDictSyncList[msg].Add(messageReturnDict[msg].First(m => m.action == f));
                }
                else
                {
                    messageReturnDictSyncList.Add(msg, new List<MessageAction<MessageReturnType>>());
                    messageReturnDictSyncList[msg].Add(messageReturnDict[msg].First(m => m.action == f));
                }
            }
            else
                throw new MessagingException("You are trying to remove an action from message type " + msg + ". But noting is listening this message type.");
        }

        public void _UnsubscribeForAllMessages(Action<Message> a)
        {
            //currently has redundant check if message is registered. 
            foreach (string key in messageDict.Keys)
            {
                MessageAction inspecting = messageDict[key].FirstOrDefault(m => m.action == a);
                if (inspecting != default(MessageAction))
                {
                    _StopListening(key, a);
                }
            }
        }
        public void _UnsubscribeForAllMessages(Func<Message, MessageReturnType> f)
        {
            //currently has redundant check if message is registered. 
            foreach (string key in messageReturnDict.Keys)
            {
                MessageAction<MessageReturnType> inspecting = messageReturnDict[key].FirstOrDefault(m => m.action == f);
                if (inspecting != default(MessageAction<MessageReturnType>))
                {
                    _StopListening(key, f);
                }
            }
        }
        /// <summary>
        /// Stops listening for all message which arent flagged as "persist"
        /// </summary>
        public void SafeFlush()
        {
            foreach (string msg in messageDict.Keys)
            {
                foreach (MessageAction action in messageDict[msg])
                {
                    if (messageDictSyncList.ContainsKey(msg))
                    {
                        MessageAction messageActionInSyncList = messageDictSyncList[msg].FirstOrDefault(m => m.action == action.action);

                        if (!action.persist &&
                            messageActionInSyncList == default(MessageAction))
                        {
                            _StopListening(msg, action.action);
                        }
                    }
                    else
                    {
                        if (!action.persist)
                            _StopListening(msg, action.action);
                    }
                }
            }
        }
        /// <summary>
        /// Empties the queue and listener list. Will not remove listeners which have been set to persist.
        /// Will definitely flush the message queue. All Messages pushed to the queue before this gets flushed will be deleted.
        /// </summary>
        public void Flush()
        {
            Dictionary<string, List<MessageAction>> persisting_messages = new Dictionary<string, List<MessageAction>>();
            Dictionary<string, List<MessageAction>> persisting_syncs = new Dictionary<string, List<MessageAction>>();
            Dictionary<string, List<MessageAction>> persisting_adds = new Dictionary<string, List<MessageAction>>();

            //Copy Messages from MessageDict flagged to persist flushing.
            foreach (string m in messageDict.Keys)
            {
                bool addflag = false;
                foreach (MessageAction ma in messageDict[m])
                {
                    if (ma.persist)
                    {
                        if (!addflag)
                        {
                            persisting_messages.Add(m, new List<MessageAction>());
                            addflag = true;
                        }
                        persisting_messages[m].Add(new MessageAction(ma));
                    }
                }
            }
            //Copy Messages from MessageDictSyncList flagged to persist flushing.
            foreach (string m in messageDictSyncList.Keys)
            {
                bool addflag = false;
                foreach (MessageAction ma in messageDictSyncList[m])
                {
                    if (ma.persist)
                    {
                        if (!addflag)
                        {
                            persisting_syncs.Add(m, new List<MessageAction>());
                            addflag = true;
                        }
                        persisting_syncs[m].Add(new MessageAction(ma));
                    }
                }
            }
            //Copy Messages from MessageDictAddList flagged to persist flushing.
            foreach (string m in messageDictAddList.Keys)
            {
                bool addflag = false;
                foreach (MessageAction ma in messageDictAddList[m])
                {
                    if (ma.persist)
                    {
                        if (!addflag)
                        {
                            persisting_adds.Add(m, new List<MessageAction>());
                            addflag = true;
                        }
                        persisting_adds[m].Add(new MessageAction(ma));
                    }
                }
            }
            messageQueue.Clear();
            messageDict.Clear();
            messageDictSyncList.Clear();
            messageDictAddList.Clear();

            messageDict = persisting_messages;
            messageDictAddList = persisting_adds;
            messageDictSyncList = persisting_syncs;
        }
        /// <summary>
        /// Subscribes an action to a string. The action gets invoked everytime the messageQueue is popped.
        /// References GameManager.instance.messageHub.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        /// <param name="p">The priority with which the message shoudl be invoked</param>
        /// <param name="persist">Whether the Message should persist flushing (false by default)</param>
        public static void StartListening(string msg, Action<Message> a, MethodPriority p, bool persist)
        {
            try
            {
                _distributor.messageHub._StartListening(msg, a, (int)p, persist);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }
        /// <summary>
        /// Subscribes an action to a string. The action gets invoked everytime the messageQueue is popped.
        /// References GameManager.instance.messageHub.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        /// <param name="p">The priority with which the message shoudl be invoked</param>
        public static void StartListening(string msg, Action<Message> a, MethodPriority p)
        {
            try
            {
                _distributor.messageHub._StartListening(msg, a, (int)p);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }
        /// <summary>
        /// Subscribes an action to a string. The action gets invoked everytime the messageQueue is popped.
        /// References GameManager.instance.messageHub.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        /// <param name="p">The priority with which the message shoudl be invoked</param>
        public static void StartListening(string msg, Action<Message> a, int priority)
        {
            try
            {
                _distributor.messageHub._StartListening(msg, a, priority);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }
        /// <summary>
        /// Subscribes an action to a string. The action gets invoked everytime the messageQueue is popped.
        /// References GameManager.instance.messageHub.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        /// <param name="p">The priority with which the message shoudl be invoked</param>
        [Obsolete]
        public static void StartListeningWithReturn(string msg, Func<Message, MessageReturnType> f, MethodPriority p)
        {
            try
            {
                _distributor.messageHub._StartListening(msg, f, p);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }

        /// <summary>
        /// Subscribes an action to a string. The action gets invoked everytime the messageQueue is popped.
        /// References GameManager.instance.messageHub.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        /// <param name="persist">Whether the Message should persist flushing (false by default)</param>
        public static void StartListening(string msg, Action<Message> a, bool persist)
        {
            try
            {
                _distributor.messageHub._StartListening(msg, a, (int)MethodPriority.Default, persist);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }
        /// <summary>
        /// Subscribes an action to a string. The action gets invoked everytime the messageQueue is popped.
        /// References GameManager.instance.messageHub.
        /// Uses Default message priority.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        public static void StartListening(string msg, Action<Message> a)
        {
            try
            {
                _distributor.messageHub._StartListening(msg, a);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }
        /// <summary>
        /// Unsubscribes an action from all message types it is registered to.
        /// </summary>
        /// <param name="a"></param>
        public static void UnsubscribeForAllMessages(Action<Message> a)
        {
            try
            {
                _distributor.messageHub._UnsubscribeForAllMessages(a);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }
        /// <summary>
        /// Call to remove an action from given message type.
        /// References GameManager.instance.messageHub.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="a"></param>
        public static void StopListening(string msg, Action<Message> a)
        {
            try
            {
                _distributor.messageHub._StopListening(msg, a);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }
        /// <summary>
        /// Call to fire the next message in the queue.
        /// First removes all registered messages in messageDictSyncList then fires the message.
        /// This enables handlers to be removed as result of a message.
        /// </summary>
        /// <returns>true once the queue has been popped sucessfully</returns>
        public bool _DequeueAndInvoke()
        {
            if (messageDictSyncList.Keys.Count > 0)
            {
                foreach (string msgType in messageDictSyncList.Keys)
                {
                    for (int i = 0; i < messageDictSyncList[msgType].Count; i++)
                    {
                        MessageAction msgAction = messageDict[msgType].FirstOrDefault(a => a == messageDictSyncList[msgType][i]);
                        if (msgAction != default(MessageAction))
                        {
                            messageDict[msgType].Remove(msgAction);
                            if (messageDict[msgType].Count == 0)
                                messageDict.Remove(msgType);
                        }
                    }

                }
                messageDictSyncList.Clear();
            }
            
            UpdateListeners();
            if (messageQueue.Count <= 0)
                return false;
            Message msg = messageQueue.Dequeue();
            if (messageDict.ContainsKey(msg.type) && messageDict[msg.type].Count > 0)
            {
                for (int i = 0; i < messageDict[msg.type].Count; i++)
                {
                    messageDict[msg.type][i].Invoke(msg);
                }
            }

            return true;
        }
        /// <summary>
        /// Call to fire the next message in the queue.
        /// First removes all registered messages in messageDictSyncList then fires the message.
        /// This enables handlers to be removed as result of a message.
        /// </summary>
        /// <returns>true once the queue has been popped sucessfully</returns>
        public bool _DequeueAndInvokeReturnQueue()
        {
            if (messageReturnDictSyncList.Keys.Count > 0)
            {
                foreach (string msgType in messageReturnDictSyncList.Keys)
                {
                    for (int i = 0; i < messageReturnDictSyncList[msgType].Count; i++)
                    {
                        MessageAction<MessageReturnType> msgAction = messageReturnDict[msgType].FirstOrDefault(a => a == messageReturnDictSyncList[msgType][i]);
                        if (msgAction != default(MessageAction<MessageReturnType>))
                        {
                            messageReturnDict[msgType].Remove(msgAction);
                            if (messageReturnDict[msgType].Count == 0)
                                messageReturnDict.Remove(msgType);
                        }
                    }

                }
                messageReturnDictSyncList.Clear();
            }

            UpdateReturnListeners();

            Message msg = messageReturnQueue.Dequeue();
            if (messageReturnDict.ContainsKey(msg.type) && messageReturnDict[msg.type].Count > 0)
            {
                for (int i = 0; i < messageReturnDict[msg.type].Count; i++)
                {
                    MessageReturnType returnType = messageReturnDict[msg.type][i].Invoke(msg);

                    if (returningMessageFunctionPointers[msg.type].Count > 0)
                        returningMessageFunctionPointers[msg.type].Dequeue().Invoke(returnType);
                    else
                        throw new MessagingException("You tried to dequeue a message with a callback but there was no callback function registered.");
                }
            }

            return true;
        }

        public void UpdateListeners()
        {
            if (messageDictAddList.Keys.Count > 0)
            {
                foreach (string msgType in messageDictAddList.Keys)
                {
                    for (int i = 0; i < messageDictAddList[msgType].Count; i++)
                    {

                        if (messageDict.ContainsKey(msgType))
                        {
                            MessageAction msgAction = messageDict[msgType].FirstOrDefault(a => a == messageDictAddList[msgType][i]);
                            if (msgAction != default(MessageAction))
                                throw new MessagingException("You are trying to add the same action to the same message type twice.");
                            messageDict[msgType].Add(messageDictAddList[msgType][i]);
                        }
                        else
                        {
                            messageDict.Add(msgType, new List<MessageAction> { messageDictAddList[msgType][i] });
                        }
                    }
                    messageDict[msgType] = messageDict[msgType].OrderByDescending(m => m.priority).ToList();
                }
            }

            messageDictAddList.Clear();
        }

        public void UpdateReturnListeners()
        {
            if (messageReturnDictAddList.Keys.Count > 0)
            {
                foreach (string msgType in messageReturnDictAddList.Keys)
                {
                    for (int i = 0; i < messageReturnDictAddList[msgType].Count; i++)
                    {

                        if (messageReturnDict.ContainsKey(msgType))
                        {
                            MessageAction<MessageReturnType> msgAction = messageReturnDict[msgType].FirstOrDefault(a => a == messageReturnDictAddList[msgType][i]);
                            if (msgAction != default(MessageAction<MessageReturnType>))
                                throw new MessagingException("You are trying to add the same action to the same message type twice.");
                            messageReturnDict[msgType].Add(messageReturnDictAddList[msgType][i]);
                        }
                        else
                        {
                            messageReturnDict.Add(msgType, new List<MessageAction<MessageReturnType>> { messageReturnDictAddList[msgType][i] });
                        }
                    }
                    messageReturnDict[msgType] = messageReturnDict[msgType].OrderByDescending(m => m.priority).ToList();
                }
            }

            messageReturnDictAddList.Clear();
        }
        /// <summary>
        /// Call to enqueue a message.
        /// </summary>
        /// <param name="msg"></param>
        public void _PushQueue(Message msg)
        {
            messageQueue.Enqueue(msg);
        }
        /// <summary>
        /// Call to enqueue a message.
        /// </summary>
        /// <param name="msg"></param>
        public void _PushReturnQueue(Message msg, Action<MessageReturnType> returnAction)
        {
            messageReturnQueue.Enqueue(msg);
            if (!returningMessageFunctionPointers.Keys.Contains(msg.type))
                returningMessageFunctionPointers.Add(msg.type, new Queue<Action<MessageReturnType>>());

            returningMessageFunctionPointers[msg.type].Enqueue(returnAction);
            //Action<MessageReturnType> retAction = returningMessageFunctionPointers[msg.type].FirstOrDefault(a => a == returnAction);

        }
        /// <summary>
        /// Call to enqueue a message.
        /// </summary>
        /// <param name="msg"></param>
        public static void Enqueue(Message msg)
        {
            try
            {
                _distributor.messageHub._PushQueue(msg);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }

        /// <summary>
        /// Works similarly to PopQueue allthoug is not asynchronous.
        /// </summary>
        /// <param name="msg"></param>
        public void BroadcastImmediate(Message msg)
        {
            if (messageDictSyncList.Keys.Count > 0)
            {
                foreach (string msgType in messageDictSyncList.Keys)
                {
                    for (int i = 0; i < messageDictSyncList[msgType].Count; i++)
                    {
                        MessageAction msgAction = messageDict[msgType].FirstOrDefault(a => a == messageDictSyncList[msgType][i]);
                        if (msgAction != default(MessageAction))
                        {
                            messageDict[msgType].Remove(msgAction);
                            if (messageDict[msgType].Count == 0)
                                messageDict.Remove(msgType);
                        }
                    }
                }
                messageDictSyncList.Clear();
            }

            UpdateListeners();

            if (messageDict.ContainsKey(msg.type) && messageDict[msg.type].Count > 0)
            {
                for (int i = 0; i < messageDict[msg.type].Count; i++)
                {
                    messageDict[msg.type][i].Invoke(msg);
                }
            }
        }
        /// <summary>
        /// The Return Queue will only work at default priority.
        /// Allows for easy returning of data which has been processed by a message.
        /// Is a quick implementation, may still be buggy.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="returnHook"></param>
        public static void PushReturnQueue(Message msg, Action<MessageReturnType> returnHook)
        {
            try
            {
                _distributor.messageHub._PushReturnQueue(msg, returnHook);
            }
            catch (NullReferenceException e)
            {
                throw new MessagingException(e.Message, e);
            }
        }
    } 
}