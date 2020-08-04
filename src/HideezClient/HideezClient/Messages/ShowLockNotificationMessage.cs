using HideezClient.Controls;
using HideezClient.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Messages
{
    class ShowLockNotificationMessage
    {
        public string NotificationId { get; }

        public string Message { get; }

        public string Title { get; }

        public NotificationOptions Options { get; }

        public ShowLockNotificationMessage(string message = null, string title = null, NotificationOptions options = null, string notificationId = null)
        {
            Message = message;
            Title = title;
            Options = options;
            NotificationId = notificationId;
        }
    }
}
