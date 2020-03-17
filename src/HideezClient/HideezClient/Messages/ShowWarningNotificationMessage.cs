using HideezClient.Modules;

namespace HideezClient.Messages
{
    class ShowWarningNotificationMessage
    {
        public string NotificationId { get; }

        public string Message { get; }

        public string Title { get; }

        public NotificationOptions Options { get; }

        public ShowWarningNotificationMessage(string message = null, string title = null, NotificationOptions options = null, string notificationId = null)
        {
            Message = message;
            Title = title;
            Options = options;
            NotificationId = notificationId;
        }
    }
}
