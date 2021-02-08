using HideezMiddleware.Settings;
using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public class LoadUserProximitySettingsMessageReply: PubSubMessageBase
    {
        public UserDeviceProximitySettings UserDeviceProximitySettings { get; set; }
        public LoadUserProximitySettingsMessageReply(UserDeviceProximitySettings userDeviceProximitySettings)
        {
            UserDeviceProximitySettings = userDeviceProximitySettings;
        }
    }
}
