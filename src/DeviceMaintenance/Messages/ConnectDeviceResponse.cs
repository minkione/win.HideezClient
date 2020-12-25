using Hideez.SDK.Communication.Connection;
using Hideez.SDK.Communication.Interfaces;
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
