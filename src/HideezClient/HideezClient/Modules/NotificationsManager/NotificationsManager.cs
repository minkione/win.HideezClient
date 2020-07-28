using HideezClient.Controls;
using HideezClient.Utilities;
using HideezClient.ViewModels;
using HideezClient.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HideezClient.Modules.NotificationsManager
{
    class NotificationsManager: INotificationsManager
    {
        const string DEFAULT_NOTIFICATION_ID = "DEFAULT_ID";
        readonly object lockObj = new object();

        Dictionary<string, NotificationsContainerWindow> windowsForNotifications = new Dictionary<string, NotificationsContainerWindow>();
        public NotificationsManager()
        {
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            ClearContainers();
        }

        public void ShowNotification(string notificationId, string title, string message, NotificationIconType notificationType, NotificationOptions options)
        {
            CreateNotification(notificationId, title, message, options, notificationType);
        }

        public void ClearNotifications()
        {
            var notifications = GetNotifications();
            foreach (NotificationBase notification in notifications)
            {
                notification.Close();
            }
        }

        void CreateNotification(string notificationId, string title, string message, NotificationOptions options, NotificationIconType notificationType)
        {
            Screen screen = GetCurrentScreen();

            // Do not create notifications without content
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (string.IsNullOrWhiteSpace(notificationId))
                notificationId = DEFAULT_NOTIFICATION_ID;


            SimpleNotification notification = new SimpleNotification(options ?? new NotificationOptions(), notificationType)
            {
                DataContext = new SimpleNotificationViewModel { Title = title, Message = message, ObservableId = notificationId, }
            };

            if (notification.Options.IsReplace)
            {
                var matchingNotificationViews = GetNotifications().Where(n => (n.DataContext as SimpleNotificationViewModel)?.ObservableId == notificationId);
                foreach (var notificationView in matchingNotificationViews)
                {
                    if (notificationView.DataContext is SimpleNotificationViewModel matchingNotificationViewModel)
                    {
                        //Close old notification when:
                        //1) Notification with matching ID is found but this ID is not the default
                        //2) Notification with default ID and matching content is found
                        if (matchingNotificationViewModel.ObservableId == notificationId)
                        {
                            if(matchingNotificationViewModel.ObservableId != DEFAULT_NOTIFICATION_ID)
                                notificationView.Close();
                            else 
                            if (matchingNotificationViewModel.Message == message && matchingNotificationViewModel.Title == title)
                            notificationView.Close();
                        }
                    }
                }
            }

            AddNotification(screen, notification);
        }

        /// <summary>
        /// Find container for notification by screen if not found container create new and then add notification to container associated with the screen
        /// </summary>
        /// <param name="screen">Screen where show notification</param>
        /// <param name="notification">Notification</param>
        /// <param name="addForce">If tru Add to stack notifications if count notification more then max</param>
        void AddNotification(Screen screen, NotificationBase notification, bool addForce = false)
        {
            NotificationsContainerWindow window = null;
            lock (lockObj)
            {
                windowsForNotifications.TryGetValue(screen.DeviceName, out window);
                if (window == null)
                {
                    window = new NotificationsContainerWindow(screen);
                    window.Show();

                    windowsForNotifications[screen.DeviceName] = window;
                }
            }

            (window.DataContext as NotificationsContainerViewModel)?.AddNotification(notification, addForce);
            window.Show();
        }

        Screen GetCurrentScreen()
        {
            IntPtr foregroundWindow = Win32Helper.GetForegroundWindow();
            Screen screen = Screen.FromHandle(foregroundWindow);
            return screen;
        }

        IEnumerable<NotificationBase> GetNotifications()
        {
            return windowsForNotifications
                .Values
                .OfType<NotificationsContainerWindow>()
                .SelectMany(vm => (vm.DataContext as NotificationsContainerViewModel).Items)
                .ToList();
        }

        /// <summary>
        /// Close all windows for notification if screens is not valid
        /// Example: disconnect one or more monitors
        /// </summary>
        void ClearContainers()
        {
            try
            {
                lock (lockObj)
                {
                    foreach (var screen in windowsForNotifications.Keys.Except(Screen.AllScreens.Select(s => s.DeviceName)).ToArray())
                    {
                        if (windowsForNotifications.TryGetValue(screen, out NotificationsContainerWindow window))
                        {
                            window.Close();
                            windowsForNotifications.Remove(screen);
                        }
                    }
                }
            }
            catch { }
        }
    }
}
