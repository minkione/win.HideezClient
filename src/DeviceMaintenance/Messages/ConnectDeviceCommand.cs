using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class ConnectDeviceCommand : PubSubMessageBase
    {
        public string Mac { get; internal set; }

        public ConnectDeviceCommand(string mac)
        {
            Mac = mac;
        }
    }
}
