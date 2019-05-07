using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class ConnectionChangedMessage
    {
        public ConnectionChangedMessage(bool isConnected)
        {
            this.IsConnected = isConnected;
        }

        public bool IsConnected { get; }
    }
}
