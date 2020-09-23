using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.IPC.Messages.RemoteDevice
{
    /// <summary>
    /// Message to notify service about disconnecting remote device.
    /// </summary>
    public class RemoteDeviceDisconnectedMessage: PubSubMessageBase
    {
        /// <summary>
        /// MetaPubSub through which service communicates with the remote device.
        /// </summary>
        public IMetaPubSub RemoteConnectionPubSub { get; set; }

        public RemoteDeviceDisconnectedMessage(IMetaPubSub remoteConnectionPubSub)
        {
            RemoteConnectionPubSub = remoteConnectionPubSub;
        }
    }
}
