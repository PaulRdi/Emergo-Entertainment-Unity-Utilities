using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergoEntertainment.Messaging
{
    public class MessagingException : Exception
    {
        public MessagingException()
        {

        }
        public MessagingException(string message) : base(message)
        {

        }
        public MessagingException(string message, Exception innerException) : base(message, innerException)
        {

        }

    }
}
