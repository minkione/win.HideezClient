using HideezClient.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Modules.NotificationsManager
{
    interface INotificationsManager
    {
        void ShowNotification(string notificationId, string title, string message, NotificationIconType notificationType, NotificationOptions options = null);
        void ClearNotifications();
    }
}
