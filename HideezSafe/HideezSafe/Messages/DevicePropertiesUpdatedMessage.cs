using HideezSafe.HideezServiceReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class DevicePropertiesUpdatedMessage
    {
        public DevicePropertiesUpdatedMessage(DeviceDTO device)
        {
            this.Device = device;
        }

        public DeviceDTO Device { get; set; }
    }
}
