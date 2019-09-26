using DynamicData;
using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Utilities;
using MvvmExtensions.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace HideezClient.ViewModels
{
    class CredentialsInfoViewModel : ReactiveObject
    {
        public CredentialsInfoViewModel(AccountRecord accountRecord)
        {
            Name = accountRecord.Name;
            Login = accountRecord.Login;
            HasOpt = accountRecord.HasOtp;
            AppsUrls = new List<string>();

            if (accountRecord.Apps != null)
            {
                AppsUrls.AddRange(AccountUtility.Split(accountRecord.Apps));
            }
            if (accountRecord.Urls != null)
            {
                AppsUrls.AddRange(AccountUtility.Split(accountRecord.Urls));
            }
        }

        public string Name { get; }
        public string Login { get; }
        public bool HasOpt { get; }
        public IList<string> AppsUrls { get; }

        #region Command

        public ICommand EditCredentialsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnEditCredentials();
                    },
                };
            }
        }

        #endregion

        private void OnEditCredentials()
        {
        }
    }
}
