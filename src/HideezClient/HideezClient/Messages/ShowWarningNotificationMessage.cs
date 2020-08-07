using HideezClient.Modules;
using Meta.Lib.Modules.PubSub;

namespace HideezClient.Messages
{
    public sealed class ShowWarningNotificationMessage: PubSubMessageBase
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
