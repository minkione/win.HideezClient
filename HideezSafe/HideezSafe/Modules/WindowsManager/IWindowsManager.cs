using HideezSafe.ViewModels;
using HideezSafe.Modules.ActionHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    public interface IWindowsManager
    {
        void ActivateMainWindow();
        Task ActivateMainWindowAsync();
        event EventHandler<bool> MainWindowVisibleChanged;
        bool IsMainWindowVisible { get; }
        void ShowDialogAddCredential(DeviceViewModel device);
        void ShowError(string message);
        void ShowWarning(string message);
        Task<Account> SelectAccountAsync(Account[] accounts);
    }
}
