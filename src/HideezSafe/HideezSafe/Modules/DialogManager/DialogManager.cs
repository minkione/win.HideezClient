using HideezSafe.Dialogs;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HideezSafe.Modules
{
    enum DialogType
    {
        ChangePassword,
    }


    class DialogManager : IDialogManager
    {
        private readonly IDictionary<BaseMetroDialog, object> dialogs = new Dictionary<BaseMetroDialog, object>();

        public void ShowDialog(DialogType dialogType)
        {
            if (System.Windows.Application.Current.MainWindow is MetroWindow metroWindow)
            {
                switch (dialogType)
                {
                    case DialogType.ChangePassword:
                        metroWindow.ShowMetroDialogAsync(new ChangePasswordDialog());
                        break;
                }

            }
        }
    }
}
