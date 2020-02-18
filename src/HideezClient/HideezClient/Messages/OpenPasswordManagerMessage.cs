namespace HideezClient.Messages
{

    class OpenPasswordManagerMessage
    {
        public string DeviceId { get; }

        public OpenPasswordManagerMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
