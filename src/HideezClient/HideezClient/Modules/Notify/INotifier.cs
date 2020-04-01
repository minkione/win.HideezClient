using System;
using HideezClient.Controls;
using HideezClient.Models;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    interface INotifier
    {
        void ShowInfo(string title, string message, NotificationOptions options = null);
        void ShowInfo(string notificationId, string title, string message, NotificationOptions options = null);

        void ShowWarn(string title, string message, NotificationOptions options = null);
        void ShowWarn(string notificationId, string title, string message, NotificationOptions options = null);

        void ShowError(string title, string message, NotificationOptions options = null);
        void ShowError(string notificationId, string title, string message, NotificationOptions options = null);

        void ShowStorageLoadingNotification(CredentialsLoadNotificationViewModel viewModel);
        void ShowDeviceNotAuthorizedNotification(HardwareVaultModel device);
        void ShowDeviceIsLockedNotification(HardwareVaultModel device);
        Task<Account> SelectAccountAsync(Account[] accounts, IntPtr hwnd);
    }
}