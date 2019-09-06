namespace HideezClient.Messages
{
    class ShowButtonConfirmUiMessage
    {
        public string DeviceId { get; }

        public ShowButtonConfirmUiMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
