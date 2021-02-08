using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class LoadUserProximitySettingsMessage: PubSubMessageBase
    {
        public string DeviceConnectionId { get; set; }

        public LoadUserProximitySettingsMessage(string connectionId)
        {
            DeviceConnectionId = connectionId;
        }
    }
}
