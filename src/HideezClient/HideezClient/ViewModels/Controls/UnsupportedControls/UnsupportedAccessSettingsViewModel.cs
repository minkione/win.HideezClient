using HideezMiddleware.Localize;
using MahApps.Metro.IconPacks;
using MvvmExtensions.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace HideezClient.ViewModels.Controls.UnsupportedControls
{
    class UnsupportedAccessSettingsViewModel : ReactiveObject
    {
        #region Property
        [Reactive] public string Text { get; set; }

        [Reactive] public string ButtonText { get; set; }

        [Reactive] public PackIconMaterialKind Icon { get; set; }

        [Reactive] public bool IsVisibleButton { get; set; } = true;
        #endregion

        #region Commands
        public ICommand OpenUpdateAppCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        Task.Run(OpenMaintenance);
                    }
                };
            }
        }
        #endregion

        public UnsupportedAccessSettingsViewModel()
        {
            Text = string.Format(TranslationSource.Instance["AccessSettings.Unsupported"], new Version(3,6,10));
            ButtonText = TranslationSource.Instance["AccessSettings.UpdateFW"];
            Icon = PackIconMaterialKind.OpenInApp;
        }

        void OpenMaintenance()
        {
            Process.Start("C:/Program Files/Hideez/Client/Hideez Device Maintenance Application.exe");
        }
    }
}
