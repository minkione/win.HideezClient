namespace HideezClient.Messages
{

    class CancelActivationCodeEntryMessage
    {
        public string DeviceId { get; }

        public CancelActivationCodeEntryMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
