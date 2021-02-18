using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.MasterPassword
{
    internal sealed class SendMasterPasswordMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public byte[] Password { get; }

        public byte[] OldPassword { get; }

        public SendMasterPasswordMessage(string deviceId, byte[] password, byte[] oldPassword = null)
        {
            DeviceId = deviceId;
            Password = password;
            OldPassword = oldPassword;
        }
    }
}
