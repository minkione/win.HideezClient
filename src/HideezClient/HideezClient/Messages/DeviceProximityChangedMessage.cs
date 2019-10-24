namespace HideezClient.Messages
{
    class DeviceProximityChangedMessage
    {
        public DeviceProximityChangedMessage(string deviceId, double proximity)
        {
            DeviceId = deviceId;
            Proximity = proximity;
        }

        public string DeviceId { get; private set; }

        public double Proximity { get; private set; }
    }
}
