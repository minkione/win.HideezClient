using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class ConnectionHESChangedMessage : ConnectionChangedMessage
    {
        public ConnectionHESChangedMessage(bool isConnected)
            : base(isConnected)
        {
        }
    }
}
