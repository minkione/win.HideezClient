using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class ConnectionDongleChangedMessage : ConnectionChangedMessage
    {
        public ConnectionDongleChangedMessage(bool isConnected)
            : base(isConnected)
        {
        }
    }
}
