using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class ConnectionServiceChangedMessage : ConnectionChangedMessage
    {
        public ConnectionServiceChangedMessage(bool isConnected)
            : base(isConnected)
        {
        }
    }
}
