using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class ShowButtonConfirmUiMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public ShowButtonConfirmUiMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
