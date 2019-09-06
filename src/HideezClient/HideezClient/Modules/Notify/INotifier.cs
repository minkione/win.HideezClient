using System;
using HideezClient.Controls;
using HideezClient.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HideezClient.Modules
{
    interface INotifier
    {
        void ShowInfo(string title, string message, NotificationOptions options = null);
        void ShowWarn(string title, string message, NotificationOptions options = null);
        void ShowError(string title, string message, NotificationOptions options = null);
        void ShowCredentialsLoading(CredentialsLoadNotificationViewModel viewModel);
        void ShowDeviceNotAuthorized(Device device);
        Task<Account> SelectAccountAsync(Account[] accounts, IntPtr hwnd);
        IEnumerable<NotificationBase> GetNotifications(Guid id);
        void CloseNotifications(Guid id);
    }
}