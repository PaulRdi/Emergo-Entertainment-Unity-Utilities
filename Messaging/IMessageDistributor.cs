using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergoEntertainment.Messaging
{
    public interface IMessageDistributor
    {
        MessageHub messageHub { get; set; }
        bool DequeueMessage();
    }
}
