using Hideez.SDK.Communication.Interfaces;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class ConnectDeviceResponse : PubSubMessageBase
    {
        public IDevice Device { get; }
        public string Mac { get; }

        public ConnectDeviceResponse(IDevice device, string mac)
        {
            Device = device;
            Mac = mac;
        }
    }
}
