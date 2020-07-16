using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class UserErrorMessage : PubSubMessageBase
    {
        public string NotificationId { get; }

        public string Message { get; }

        public UserErrorMessage(string notificationId, string message)
        {
            NotificationId = notificationId;
            Message = message;
        }
    }
}
