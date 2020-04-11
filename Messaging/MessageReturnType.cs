using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergoEntertainment.Messaging
{
    public class MessageReturnType
    {
        public object data;
        public MessageReturnType(object v) { data = v; }
    }
}
