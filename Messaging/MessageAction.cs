using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergoEntertainment.Messaging
{
    public class MessageAction
    {
        Action<Message> the_action;
        public Action<Message> action { get { return the_action; } }
        public int priority;
        public bool persist;
        public MessageAction(Action<Message> a, MethodPriority p, bool persist)
        {
            the_action = a;
            this.persist = persist;
            priority = (int)p;
        }
        public MessageAction(Action<Message> a, int p, bool persist)
        {
            the_action = a;
            this.persist = persist;
            priority = (int)p;
        }
        public MessageAction(MessageAction other)
        {
            the_action = other.the_action;
            priority = other.priority;
            persist = other.persist;
        }

        public void Invoke(Message m)
        {
            the_action.Invoke(m);
        }
    }

    public class MessageAction<T> where T : MessageReturnType
    {
        Func<Message, T> the_action;
        public Func<Message, T> action { get { return the_action; } }
        public int priority;
        public bool persist;
        public MessageAction(Func<Message, T> f, MethodPriority p, bool _persist)
        {
            the_action = f;
            persist = _persist;
            priority = (int)p;
        }
        public MessageAction(MessageAction<T> other)
        {
            the_action = other.the_action;
            priority = other.priority;
            persist = other.persist;
        }

        public T Invoke(Message m)
        {
            return the_action.Invoke(m);
        }
    }
}
