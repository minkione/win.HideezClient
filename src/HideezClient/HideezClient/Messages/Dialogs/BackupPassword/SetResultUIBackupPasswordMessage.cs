using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages.Dialogs.BackupPassword
{
    internal class SetResultUIBackupPasswordMessage: PubSubMessageBase
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage{ get; set; }

        public SetResultUIBackupPasswordMessage(bool isSuccessful, string errorMessage = "")
        {
            IsSuccessful = isSuccessful;
            ErrorMessage = errorMessage;
        }
    }
}
