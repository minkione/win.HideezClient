using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class UserNotificationMessage : PubSubMessageBase
    {
        public string NotificationId { get; }

        public string Message { get; }

        public UserNotificationMessage(string notificationId, string message)
        {
            NotificationId = notificationId;
            Message = message;
        }
    }
}
