using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMaintenance.Messages
{
    public class MessageBase// : IPubSubMessage
    {
        public bool DeliverAtLeastOnce => true;

        public int Timeout => 1000;
    }
}
