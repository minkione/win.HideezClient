using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class ShowActivationCodeUiMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public ShowActivationCodeUiMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
