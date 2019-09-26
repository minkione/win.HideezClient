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
using DynamicData;
using Hideez.ARM;
using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Modules;
using HideezClient.Utilities;
using MvvmExtensions.Commands;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HideezClient.ViewModels
{
    class EditCredentialsViewModel : ReactiveObject
    {
        private bool isUpdateAppsUrls;

        public EditCredentialsViewModel()
        {
            InitDependencies();
        }

        public EditCredentialsViewModel(AccountRecord accountRecord)
        {
            InitProp(accountRecord);
            InitDependencies();
        }

        private void InitDependencies()
        {
            this.WhenAnyValue(vm => vm.Name, vm => vm.Login, vm => vm.Password, vm => vm.HasOpt, vm => vm.OtpSecret)
                .Where(_ => IsEditable)
                .Subscribe(_ => HasChanges = true);

            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Apps, nameof(ObservableCollection<string>.CollectionChanged))
                       .Where(_ => IsEditable)
                      .Subscribe(change => HasChanges = true);
            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Urls, nameof(ObservableCollection<string>.CollectionChanged))
                       .Where(_ => IsEditable)
                      .Subscribe(change => HasChanges = true);

            UpdateAppsAndUrls();
        }

        private void UpdateAppsAndUrls()
        {
            if (isUpdateAppsUrls) return;

            try
            {
                isUpdateAppsUrls = true;
                var allApps = AppInfoFactory.GetVisibleAppsInfo();
                var appInfoComparer = new AppInfoComparer();

                var apps = allApps
                    .Where(a => string.IsNullOrEmpty(a.Domain))
                    .Except(OpenedApps, appInfoComparer)
                    .ToArray();

                var urls = AddMainDomain(allApps.Where(a => !string.IsNullOrEmpty(a.Domain)))
                            .Distinct(appInfoComparer)
                            .Except(OpenedForegroundUrls, appInfoComparer)
                            .ToArray();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    OpenedApps.Add(apps);
                    OpenedForegroundUrls.Add(urls);
                });
            }
            finally
            {
                isUpdateAppsUrls = false;
            }
        }

        private IEnumerable<AppInfo> AddMainDomain(IEnumerable<AppInfo> urls)
        {
            List<AppInfo> mainDomainUrls = new List<AppInfo>();
            foreach (var appInfo in urls)
            {
                string mainDomain = URLHelper.GetRegistrableDomain(appInfo.Domain);
                if (!string.IsNullOrWhiteSpace(mainDomain) && appInfo.Domain != mainDomain)
                {
                    // Create a copy of AppInfoViewModel with subdomained url, but in copy 
                    // set domain property to the value of main domain
                    // That way we will have both main domain and original subdomained url available 
                    mainDomainUrls.Add(new AppInfo
                    {
                        Domain = mainDomain,
                        ProcessName = appInfo.ProcessName,
                        Description = appInfo.Description
                    });
                }
            }

            return urls.Concat(mainDomainUrls);
        }

        private void InitProp(AccountRecord accountRecord)
        {
            Name = accountRecord.Name;
            Login = accountRecord.Login;
            HasOpt = accountRecord.HasOtp;

            if (accountRecord.Apps != null)
            {
                Apps.AddRange(AccountUtility.Split(accountRecord.Apps));
            }
            if (accountRecord.Urls != null)
            {
                Urls.AddRange(AccountUtility.Split(accountRecord.Urls));
            }
        }

        [Reactive]
        public bool IsEditable { get; set; } = true;
        [Reactive]
        public bool HasChanges { get; set; }
        [Reactive]
        public string Name { get; set; }
        [Reactive]
        public string Login { get; set; }
        [Reactive]
        public SecureString Password { get; set; }
        [Reactive]
        public bool HasOpt { get; protected set; }
        [Reactive]
        public string OtpSecret { get; set; }

        public ObservableCollection<string> Apps { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Urls { get; } = new ObservableCollection<string>();

        public ObservableCollection<AppInfo> OpenedApps { get; } = new ObservableCollection<AppInfo>();
        public ObservableCollection<AppInfo> OpenedForegroundUrls { get; } = new ObservableCollection<AppInfo>();

        #region Command

        public ICommand UpdateAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnUpdateAccount();
                    },
                };
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        OnCancel();
                    },
                };
            }
        }

        public ICommand GeneratePasswordCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        GeneratePassword();
                    }
                };
            }
        }

        #endregion

        private string GeneratePassword()
        {
            return "";
        }

        private void OnCancel()
        {
            IsEditable = false;
            HasChanges = false;
        }

        private void OnUpdateAccount()
        {
            IsEditable = false;
            HasChanges = false;
        }
    }
}
