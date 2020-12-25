using Hideez.SDK.Communication.Interfaces;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class AdvertismentReceivedEvent : PubSubMessageBase
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
