using HideezSafe.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HideezSafe.ViewModels
{
    class NotificationsContainerViewModel
    {
        private readonly int maxCountNotification = 10;
        private readonly Queue<NotificationBase> notifications = new Queue<NotificationBase>();

        public ObservableCollection<NotificationBase> Items { get; } = new ObservableCollection<NotificationBase>();

        public void AddNotification(NotificationBase notificationBase)
        {
            if (Items.Count < maxCountNotification)
            {
                Add(notificationBase);
            }
            else
            {
                notifications.Enqueue(notificationBase);
            }
        }

        public void RemoveNotification(NotificationBase notificationBase)
        {
            Items.Remove(notificationBase);
            if (Items.Count < maxCountNotification && notifications.Count != 0)
            {
                var notification = notifications.Dequeue();
                Add(notification);
            }
        }

        private void Add(NotificationBase notificationBase)
        {
            int i = 0;
            for (; i < Items.Count; i++)
            {
                if (notificationBase.Position <= Items[i].Position)
                {
                    break;
                }
            }
            Items.Insert(i, notificationBase);

            void Notification_Closed(object sender, EventArgs e)
            {
                RemoveNotification(notificationBase);
                notificationBase.Closed -= Notification_Closed;
            }
            notificationBase.Closed += Notification_Closed;
        }
    }
}
