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
using System.Windows.Media;
using System.Windows.Threading;

namespace HideezSafe.Modules
{
    class Notifier : INotifier
    {
        private bool isInitialised;
        private readonly NotificationsContainerViewModel notificationsContainer;
        private NotificationsContainerWindow notificationsWindow;

        public Notifier(NotificationsContainerViewModel notificationsContainer)
        {
            this.notificationsContainer = notificationsContainer;
        }

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
                Initialise();

            SimpleNotification notification = new SimpleNotification(options ?? new NotificationOptions(), notificationType)
            {
                DataContext = new SimpleNotificationViewModel { Title = title, Message = message, }
            };
            notificationsContainer.AddNotification(notification);
        }

        private void Initialise()
        {
            if (notificationsWindow == null)
            {
                notificationsWindow = new NotificationsContainerWindow();
                notificationsWindow.Show();
                isInitialised = true;
            }
        }

        public async Task<Account> SelectAccountAsync(Account[] accounts)
        {
            if (!isInitialised)
                Initialise();

            TaskCompletionSource<bool> taskCompletionSourceForDialog = new TaskCompletionSource<bool>();

            var viewModel = new AccountSelectorViewModel(accounts);
            AccountSelector notification = new AccountSelector(new NotificationOptions { SetFocus = true, CloseWhenDeactivate = true, Position = NotificationPosition.Bottom, TaskCompletionSource = taskCompletionSourceForDialog, })
            {
                DataContext = viewModel,
            };
            notificationsContainer.AddNotification(notification, true);

            bool dialogResalt = await taskCompletionSourceForDialog.Task;
            if (dialogResalt)
            {
                return viewModel.SelectedAccount.Account;
            }

            return null;
        }
    }
}
