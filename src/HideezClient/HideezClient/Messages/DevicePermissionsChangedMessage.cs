using HideezClient.HideezServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Messages
{
    public class DevicePermissionsChangedMessage
    {
        public DevicePermissionsChangedMessage(DevicePermissionsDTO devicePermissionsDTO)
        {
            AllowEditProximitySettings = devicePermissionsDTO.AllowEditProximitySettings;
        }

        public bool AllowEditProximitySettings { get; }
    }
}
