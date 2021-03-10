using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.Wipe
{
    internal sealed class ShowWipeDialogMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public ShowWipeDialogMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
