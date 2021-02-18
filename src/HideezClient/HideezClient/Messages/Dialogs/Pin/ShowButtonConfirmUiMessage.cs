using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.Pin
{
    public class ShowButtonConfirmUiMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public ShowButtonConfirmUiMessage(string deviceId)
        {
            DeviceId = deviceId;
        }
    }
}
