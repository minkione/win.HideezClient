using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class DeviceBatteryChangedMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public string Mac { get; }

        public int Battery { get; }

        public DeviceBatteryChangedMessage(string deviceId, string mac, int battery)
        {
            DeviceId = deviceId;
            Mac = mac;
            Battery = battery;
        }
    }
}
