using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Messages
{
    public class ConnectionChangedMessage: PubSubMessageBase
    {
        public ConnectionChangedMessage(bool isConnected)
        {
            this.IsConnected = isConnected;
        }

        public bool IsConnected { get; }
    }
}
