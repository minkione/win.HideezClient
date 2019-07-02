using HideezSafe.Controls;
using HideezSafe.Models;
using HideezSafe.Modules.ActionHandler;
using HideezSafe.ViewModels;
using HideezSafe.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace HideezSafe.Modules
{
    class Notifier : INotifier
    {
        private bool isInitialised;
        private Dictionary<Screen, NotificationsContainerWindow> windowsForNotifications;

        public void ShowInfo(string title, string message, NotificationOptions options = null)
        {
            ShowSimpleNotification(title, message, options, SimpleNotificationType.Info);
        }

        public void ShowWarn(string title, string message, NotificationOptions options = null)
        {
            ShowSimpleNotification(title, message, options, SimpleNotificationType.Warn);
        }

        public void ShowError(string title, string message, NotificationOptions options = null)
        {
            ShowSimpleNotification(title, message, options, SimpleNotificationType.Error);
        }

        private void ShowSimpleNotification(string title, string message, NotificationOptions options, SimpleNotificationType notificationType)
        {
            if (!isInitialised)
            {
                Initialise();
            }

            Update();

            foreach (var window in windowsForNotifications.Values.ToArray())
            {
                SimpleNotification notification = new SimpleNotification(options ?? new NotificationOptions(), notificationType)
                {
                    DataContext = new SimpleNotificationViewModel { Title = title, Message = message, }
                };
                (window.DataContext as NotificationsContainerViewModel)?.AddNotification(notification);
            }
        }

        private void Initialise()
        {
            if (windowsForNotifications == null)
            {
                windowsForNotifications = new Dictionary<Screen, NotificationsContainerWindow>();

                foreach (var screen in Screen.AllScreens)
                {
                    var window = new NotificationsContainerWindow(screen);
                    window.Show();
                    windowsForNotifications[screen] = window;
                }

                isInitialised = true;
            }
        }

        private void Update()
        {
            foreach (var screen in windowsForNotifications.Keys.Except(Screen.AllScreens).ToArray())
            {
                if (windowsForNotifications.TryGetValue(screen, out NotificationsContainerWindow window))
                {
                    window.Close();
                }
                windowsForNotifications.Remove(screen);
            }

            foreach (var screen in Screen.AllScreens.Except(windowsForNotifications.Keys).ToArray())
            {
                var window = new NotificationsContainerWindow(screen);
                window.Show();
                windowsForNotifications[screen] = window;
            }
        }

        public async Task<Account> SelectAccountAsync(Account[] accounts, IntPtr hwnd)
        {
            if (!isInitialised)
            {
                Initialise();
            }

            Update();

            TaskCompletionSource<bool> taskCompletionSourceForDialog = new TaskCompletionSource<bool>();

            var viewModel = new AccountSelectorViewModel(accounts);
            AccountSelector notification = new AccountSelector(new NotificationOptions { SetFocus = true, CloseWhenDeactivate = true, Position = NotificationPosition.Bottom, TaskCompletionSource = taskCompletionSourceForDialog, })
            {
                DataContext = viewModel,
            };

            windowsForNotifications.TryGetValue(Screen.FromHandle(hwnd), out NotificationsContainerWindow window);
            (window.DataContext as NotificationsContainerViewModel)?.AddNotification(notification, true);

            bool dialogResalt = await taskCompletionSourceForDialog.Task;
            if (dialogResalt)
            {
                return viewModel.SelectedAccount.Account;
            }

            return null;
        }
    }
}
