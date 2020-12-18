using Hideez.SDK.Communication.Refactored.BLE;

namespace DeviceMaintenance.Messages
{
    public class ConnectDeviceCommand : MessageBase
    {
        public ConnectionId ConnectionId { get; internal set; }

        public ConnectDeviceCommand(ConnectionId connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
