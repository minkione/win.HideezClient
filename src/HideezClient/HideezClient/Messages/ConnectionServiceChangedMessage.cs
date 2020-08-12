using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Messages
{
    public class ConnectionServiceChangedMessage : ConnectionChangedMessage
    {
        public ConnectionServiceChangedMessage(bool isConnected)
            : base(isConnected)
        {
        }
    }
}
