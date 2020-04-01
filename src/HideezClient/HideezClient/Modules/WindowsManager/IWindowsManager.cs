using HideezClient.ViewModels;
using HideezClient.Modules.ActionHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideezClient.Models;
using HideezClient.Controls;
using System.Drawing;

namespace HideezClient.Modules
{
    public interface IWindowsManager
    {
        void ActivateMainWindow();
        void InitializeMainWindow();
        void HideMainWindow();
        Task ActivateMainWindowAsync();
        Task HideMainWindowAsync();
        Task InitializeMainWindowAsync();
        
        Task<Bitmap> GetCurrentScreenImageAsync();
        event EventHandler<bool> MainWindowVisibleChanged;
        bool IsMainWindowVisible { get; }
        void CloseWindow(string id);


        Task<AccountModel> SelectAccountAsync(AccountModel[] accounts, IntPtr hwnd);
        void ShowCredentialsLoading(CredentialsLoadNotificationViewModel viewModel);

        Task<bool> ShowDeleteCredentialsPromptAsync();
        Task<bool> ShowDisconnectDevicePromptAsync(string deviceName);
        Task<bool> ShowRemoveDevicePromptAsync(string deviceName);

    }
}
