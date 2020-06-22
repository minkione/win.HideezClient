namespace HideezClient.Messages
{
    class LockDeviceStorageMessage
    {
        public string SerialNo { get; }

        public LockDeviceStorageMessage(string serialNo)
        {
            SerialNo = serialNo;
        }
    }
}
