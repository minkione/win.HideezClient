using HideezClient.Modules;
using HideezClient.Mvvm;
using MahApps.Metro.Controls.Dialogs;
using MvvmExtensions.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HideezClient.PageViewModels
{
    class LockSettingsPageViewModel : ObservableObject
    {
        private readonly IDialogManager dialogManager;

        public LockSettingsPageViewModel(IDialogManager dialogManager)
        {
            this.dialogManager = dialogManager;
        }

        #region Command

        public ICommand SaveCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnSave();
                    },
                };
            }
        }

        public ICommand ChangePasswordCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnChangePassword();
                    },
                };
            }
        }

        public ICommand LockDistanceIsEnabledChangedCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnLockDistanceIsEnabledChanged();
                    },
                };
            }
        }

        #endregion Command

        #region Property

        private bool lockDistanceIsEnabled;
        private double unlockDistance = 1;
        private double lockDistance = 3;

        public bool LockDistanceIsEnabled
        {
            get { return lockDistanceIsEnabled; }
            set { Set(ref lockDistanceIsEnabled, value); }
        }

        public double UnlockDistance
        {
            get { return unlockDistance; }
            set { Set(ref unlockDistance, value); }
        }

        public double LockDistance
        {
            get { return lockDistance; }
            set { Set(ref lockDistance, value); }
        }

        #endregion Property

        private void OnLockDistanceIsEnabledChanged()
        {
            Debug.Assert(false);
        }

        private void OnChangePassword()
        {
            dialogManager.ShowDialog(DialogType.ChangePassword);
           // DialogCoordinator.Instance.ShowMessageAsync(this, "HEADER", "MESSAGE");
            // Debug.Assert(false);
        }

        private void OnSave()
        {
            Debug.Assert(false);
        }
    }
}
