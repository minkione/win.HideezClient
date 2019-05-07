using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class ConnectionRFIDChangedMessage : ConnectionChangedMessage
    {
        public ConnectionRFIDChangedMessage(bool isConnected)
            : base(isConnected)
        {
        }
    }
}
