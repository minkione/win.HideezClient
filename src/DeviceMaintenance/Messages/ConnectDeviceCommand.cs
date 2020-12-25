using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Connection;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class ConnectDeviceCommand : PubSubMessageBase
    {
        public ConnectionId ConnectionId { get; internal set; }

        public ConnectDeviceCommand(ConnectionId connectionId)
        {
            ConnectionId = connectionId;
            ResponseTimeout = SdkConfig.ConnectDeviceTimeout * 3 + SdkConfig.DeviceInitializationTimeout;
        }
    }
}
