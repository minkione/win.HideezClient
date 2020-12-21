using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Refactored.BLE;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class ConnectDeviceCommand : PubSubMessageBase
    {
        public ConnectionId ConnectionId { get; internal set; }

        public ConnectDeviceCommand(ConnectionId connectionId)
        {
            ConnectionId = connectionId;
            ResponseTimeout = SdkConfig.ConnectDeviceTimeout * 2 + SdkConfig.DeviceInitializationTimeout;
        }
    }
}
