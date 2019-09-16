﻿using HideezClient.Modules;

namespace HideezClient.Messages
{
    class ShowErrorNotificationMessage
    {
        public string NotificationId { get; }

        public string Message { get; }

        public string Title { get; }

        NotificationOptions Options { get; }

        public ShowErrorNotificationMessage(string message = null, string title = null, NotificationOptions options = null, string notificationId = null)
        {
            Message = message;
            Title = title;
            Options = options;
            NotificationId = notificationId;
        }
    }
}