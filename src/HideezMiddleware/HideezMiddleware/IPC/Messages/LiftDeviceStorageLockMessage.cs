using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class LiftDeviceStorageLockMessage : PubSubMessageBase
    {
        public string SerialNo { get; }

        public LiftDeviceStorageLockMessage(string serialNo)
        {
            SerialNo = serialNo;
        }
    }
}
