using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Mvvm;
using HideezClient.ViewModels;
using MvvmExtensions.Commands;
using System.Windows.Input;

namespace HideezClient.PageViewModels
{
    class DefaultPageViewModel : LocalizedObject
    {
        readonly IMessenger _messenger;
        readonly SoftwareUnlockSettingViewModel _softwareUnlock;

        public ICommand OpenHideezKeyPageCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) => OnOpenHideezKeyPage(),
                };
            }
        }

        public ICommand OpenMobileAuthenticatorPageCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) => OnOpenMobileAuthenticatorPage(),
                };
            }
        }

        public DefaultPageViewModel(IMessenger messenger, SoftwareUnlockSettingViewModel softwareUnlock)
        {
            _messenger = messenger;
            _softwareUnlock = softwareUnlock;
        }

        void OnOpenHideezKeyPage()
        {
            _messenger.Send(new OpenHideezKeyPageMessage());
        }

        void OnOpenMobileAuthenticatorPage()
        {
            _messenger.Send(new OpenMobileAuthenticatorPageMessage());

            // Todo: Temporary for Try&Buy
            _softwareUnlock.IsChecked = true;
        }
    }
}
