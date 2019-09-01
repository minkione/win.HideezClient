using HideezClient.Models;
using HideezClient.Modules.ActionHandler;
using System;
using System.Threading.Tasks;

namespace HideezClient.Modules
{
    interface INotifier
    {
        void ShowInfo(string title, string message, NotificationOptions options = null);
        void ShowWarn(string title, string message, NotificationOptions options = null);
        void ShowError(string title, string message, NotificationOptions options = null);
        Task<Account> SelectAccountAsync(Account[] accounts, IntPtr hwnd);
    }
}