using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.IPC.Messages.RemoteDevice
{
    public class RemoteConnection_ClientDisconnectedMessage: PubSubMessageBase
    {
        public IMetaPubSub RemoteConnectionPubSub { get; set; }

        public RemoteConnection_ClientDisconnectedMessage(IMetaPubSub remoteConnectionPubSub)
        {
            RemoteConnectionPubSub = remoteConnectionPubSub;
        }
    }
}
