using HideezClient.Modules;
using HideezClient.Mvvm;
using HideezClient.Utilities;
using HideezClient.ViewModels;
using MvvmExtensions.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HideezClient.PageViewModels
{
    class LoginSystemPageViewModel : LocalizedObject
    {
        private readonly MainViewModel mainViewModel;
        private readonly IAppHelper appHelper;

        public LoginSystemPageViewModel(MainViewModel mainViewModel, IAppHelper appHelper)
        {
            this.mainViewModel = mainViewModel;
            this.appHelper = appHelper;
        }

        #region Command

        public ICommand ShowLockSettingsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnNextPage();
                    },
                };
            }
        }

        public ICommand OpenVideoTutorial
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnUrlVideo();
                    },
                };
            }
        }

        #endregion Command
        
        private void OnUrlVideo()
        {
            appHelper.OpenUrl(L("Url.LockVideoTutorial"));
        }

        private void OnNextPage()
        {
            mainViewModel.ProcessNavRequest("LockSettingsPage");
        }
    }
}
