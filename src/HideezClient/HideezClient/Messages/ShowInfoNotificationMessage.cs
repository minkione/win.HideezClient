using HideezClient.Modules;

namespace HideezClient.Messages
{
    class ShowInfoNotificationMessage
    {
        public string Message { get; }

        public string Title { get; }

        NotificationOptions Options { get; }

        public ShowInfoNotificationMessage(string message = null, string title = null, NotificationOptions options = null)
        {
            Message = message;
            Title = title;
            Options = options;
        }
    }
}
