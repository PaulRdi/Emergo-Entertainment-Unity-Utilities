using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergoEntertainment.Messaging
{
    /// <summary>
    /// Wrapper class for messages.
    /// </summary>
    public class Message
    {
        public MessageType type;
        public object[] data;

        public Message(MessageType type_, params object[] data_)
        {
            type = type_;
            data = data_;

        }
    }
}
