using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Refactored.BLE;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class ConnectDeviceResponse : PubSubMessageBase
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
