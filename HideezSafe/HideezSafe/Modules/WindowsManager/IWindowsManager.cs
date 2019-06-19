using HideezSafe.ViewModels;
using System;
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
    }
}
