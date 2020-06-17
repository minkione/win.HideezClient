namespace HideezClient.Messages
{
    class SendActivationCodeMessage
    {
        public string DeviceId { get; }

        public byte[] Code { get; }

        public SendActivationCodeMessage(string deviceId, byte[] code)
        {
            DeviceId = deviceId;
            Code = code;
        }
    }
}
