using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class DeviceProximityChangedMessage
    {
        public DeviceProximityChangedMessage(string deviceId, double proximity)
        {
            this.DeviceId = deviceId;
            this.Proximity = proximity;
        }

        public string  DeviceId { get; }
        public double Proximity { get; }
    }
}
