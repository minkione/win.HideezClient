using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Messages
{
    class ConnectionRFIDChangedMessage : ConnectionChangedMessage
    {
        public ConnectionRFIDChangedMessage(bool isConnected)
            : base(isConnected)
        {
        }
    }
}
