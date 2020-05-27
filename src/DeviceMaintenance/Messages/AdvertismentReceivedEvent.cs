using Hideez.SDK.Communication.BLE;

namespace DeviceMaintenance.Messages
{
    public class AdvertismentReceivedEvent : MessageBase
    {
        private readonly AdvertismentReceivedEventArgs _e;

        public string DeviceId => _e.Id;
        public string DeviceName => _e.DeviceName;
        public sbyte Rssi => _e.Rssi;

        public AdvertismentReceivedEvent(AdvertismentReceivedEventArgs e)
        {
            _e = e;
        }
    }
}
