using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class ShowPinUiMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public bool WithConfirm { get; }

        public bool AskOldPin { get; }

        public ShowPinUiMessage(string deviceId, bool withConfirm = false, bool askOldPin = false)
        {
            DeviceId = deviceId;
            WithConfirm = withConfirm;
            AskOldPin = askOldPin;
        }
    }
}
