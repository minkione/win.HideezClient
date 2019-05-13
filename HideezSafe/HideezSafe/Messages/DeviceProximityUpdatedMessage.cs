using HideezSafe.HideezServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class DeviceProximityUpdatedMessage
    {
        public DeviceProximityUpdatedMessage(BleDeviceDTO device)
        {
            this.Device = device;
        }

        public BleDeviceDTO Device { get; }
    }
}
