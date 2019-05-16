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
    }
}
