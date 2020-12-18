using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Refactored.BLE;

namespace DeviceMaintenance.Messages
{
    public class ConnectDeviceResponse : MessageBase
    {
        public IDevice Device { get; }
        public ConnectionId ConnectionId { get; }

        public ConnectDeviceResponse(IDevice device, ConnectionId connectionId)
        {
            Device = device;
            ConnectionId = connectionId;
        }
    }
}
