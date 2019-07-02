using HideezSafe.ViewModels;
using HideezSafe.Modules.ActionHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HideezSafe.Models;

namespace HideezSafe.Modules
{
    public interface IWindowsManager
    {
        void ActivateMainWindow();
        Task ActivateMainWindowAsync();
        event EventHandler<bool> MainWindowVisibleChanged;
        bool IsMainWindowVisible { get; }
        void ShowDialogAddCredential(Device device);

        void ShowInfo(string message, string title = null);
        void ShowWarn(string message, string title = null);
        void ShowError(string message, string title = null);
        Task<Account> SelectAccountAsync(Account[] accounts);
    }
}
