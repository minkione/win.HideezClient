using Hideez.SDK.Communication.Interfaces;

namespace DeviceMaintenance.Messages
{
    public class ConnectDeviceResponse : MessageBase
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
