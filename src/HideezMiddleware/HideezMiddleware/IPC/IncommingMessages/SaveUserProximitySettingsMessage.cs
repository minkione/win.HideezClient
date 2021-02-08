using HideezMiddleware.Settings;
using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.IPC.IncommingMessages
{
    public sealed class SaveUserProximitySettingsMessage : PubSubMessageBase
    {
        public UserDeviceProximitySettings UserDeviceProximitySettings { get; set; }

        public SaveUserProximitySettingsMessage(UserDeviceProximitySettings userProximitySettings)
        {
            UserDeviceProximitySettings = userProximitySettings;
        }
    }
}
