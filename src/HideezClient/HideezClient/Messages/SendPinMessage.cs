namespace HideezClient.Messages
{
    class SendPinMessage
    {
        public string DeviceId { get; }

        public byte[] Pin { get; }

        public byte[] OldPin { get; }

        public SendPinMessage(string deviceId, byte[] pin, byte[] oldPin = null)
        {
            DeviceId = deviceId;
            Pin = pin;
            OldPin = oldPin;
        }
    }
}
