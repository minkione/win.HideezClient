using Meta.Lib.Modules.PubSub;


namespace HideezClient.Messages
{
    public class ShowMasterPasswordUiMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public bool ConfirmPassword { get; }

        public bool OldPassword { get; }

        public ShowMasterPasswordUiMessage(string deviceId, bool withConfirm, bool askOldPassword)
        {
            DeviceId = deviceId;
            ConfirmPassword = withConfirm;
            OldPassword = askOldPassword;
        }
    }
}
