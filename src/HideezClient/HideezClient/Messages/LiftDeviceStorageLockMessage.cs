namespace HideezClient.Messages
{
    class LiftDeviceStorageLockMessage
    {
        public string SerialNo { get; }

        public LiftDeviceStorageLockMessage(string serialNo)
        {
            SerialNo = serialNo;
        }
    }
}
