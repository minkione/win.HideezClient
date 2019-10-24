namespace HideezClient.Messages
{
    class DeviceBatteryChangedMessage
    {
        public DeviceBatteryChangedMessage(string deviceId, int battery)
        {
            DeviceId = deviceId;
            Battery = battery;
        }

        public string DeviceId { get; private set; }

        public int Battery { get; private set; }
    }
}
