namespace HideezClient.Messages
{
    class ShowActivationCodeUiMessage
    {
        public string DeviceId { get; }

        public ShowActivationCodeUiMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
