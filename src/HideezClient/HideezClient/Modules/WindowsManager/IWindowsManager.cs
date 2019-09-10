using HideezClient.ViewModels;
using HideezClient.Modules.ActionHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideezClient.Models;
using HideezClient.Controls;

namespace HideezClient.Modules
{
    public interface IWindowsManager
    {
        void ActivateMainWindow();
        Task ActivateMainWindowAsync();
        event EventHandler<bool> MainWindowVisibleChanged;
        bool IsMainWindowVisible { get; }
        void ShowDialogAddCredential(Device device);
        void ShowInfoAboutDevice(Device device);
        void CloseWindow(Guid id);

        void ShowInfo(string message, string title = null);
        void ShowWarn(string message, string title = null);
        void ShowError(string message, string title = null);
        Task<Account> SelectAccountAsync(Account[] accounts, IntPtr hwnd);
        void ShowCredentialsLoading(CredentialsLoadNotificationViewModel viewModel);
        Task ShowDeviceLockedAsync();
        void ShowDeviceNotAuthorized(Device device);

        #region PIN
        /*
        Task<bool> ShowDialogEnterPinAsync(EnterPinViewModel viewModel);
        void ShowSetPin(Device device);
        void ShowChangePin(Device device);
        */
        #endregion PIN
    }
}
