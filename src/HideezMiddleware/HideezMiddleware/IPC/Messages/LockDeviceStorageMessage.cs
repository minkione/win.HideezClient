using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class LockDeviceStorageMessage : PubSubMessageBase
    {
        public string SerialNo { get; }

        public LockDeviceStorageMessage(string serialNo)
        {
            SerialNo = serialNo;
        }
    }
}
