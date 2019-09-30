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
using Unity;

namespace HideezClient.ViewModels
{
    class EditAccountViewModel : ReactiveObject
    {
        private bool isUpdateAppsUrls;
        private DeviceViewModel device;

        public EditAccountViewModel(DeviceViewModel device)
        {
            this.device = device;
            InitDependencies();
        }

        public EditAccountViewModel(DeviceViewModel device, AccountRecord accountRecord)
        {
            this.device = device;
            InitProp(accountRecord);
            InitDependencies();
        }

        private void InitDependencies()
        {
            Application.Current.MainWindow.Activated += WeakEventHandler.Create(this, (@this, o, args) => Task.Run(@this.UpdateAppsAndUrls));

            this.WhenAnyValue(vm => vm.Name, vm => vm.Login, vm => vm.Password, vm => vm.HasOpt, vm => vm.OtpSecret)
                .Where(_ => IsEditable)
                .Subscribe(_ => HasChanges = true);

            this.WhenAnyValue(vm => vm.SelectedApp).Subscribe(OnAppSelected);
            this.WhenAnyValue(vm => vm.SelectedUrl).Subscribe(OnUrlSelected);

            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Apps, nameof(ObservableCollection<string>.CollectionChanged))
                       .Where(_ => IsEditable)
                      .Subscribe(change => AppsOrUrlsCollectonChanges());
            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(Urls, nameof(ObservableCollection<string>.CollectionChanged))
                       .Where(_ => IsEditable)
                      .Subscribe(change => AppsOrUrlsCollectonChanges());

            AppInfo loadingAppInfo = new AppInfo { Description = "Loading...", Domain = "Loading..." };
            OpenedApps.Add(loadingAppInfo);
            OpenedForegroundUrls.Add(loadingAppInfo);
            Task.Run(UpdateAppsAndUrls).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    OpenedApps.Remove(loadingAppInfo);
                    OpenedForegroundUrls.Remove(loadingAppInfo);
                });
            });
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

        [Reactive] public bool IsEditable { get; set; } = true;
        [Reactive] public bool HasChanges { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Login { get; set; }
        [Reactive] public SecureString Password { get; set; }
        [Reactive] public bool HasOpt { get; protected set; }
        [Reactive] public string OtpSecret { get; set; }

        public ObservableCollection<string> Apps { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Urls { get; } = new ObservableCollection<string>();
        public IEnumerable<string> AppsAndUrls { get { return Apps.Concat(Urls); } }
        public IEnumerable<string> Logins { get { return device?.Accounts.Select(a => a.Login).Distinct(); } }
        public ObservableCollection<AppInfo> OpenedApps { get; } = new ObservableCollection<AppInfo>();
        public ObservableCollection<AppInfo> OpenedForegroundUrls { get; } = new ObservableCollection<AppInfo>();

        [Reactive] public AppInfo SelectedApp { get; set; }
        [Reactive] public AppInfo SelectedUrl { get; set; }

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
                        OnGeneratePassword();
                    }
                };
            }
        }

        #endregion

        private string OnGeneratePassword()
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

        private void OnUrlSelected(AppInfo appInfo)
        {
            string url = appInfo?.Domain;
            if (!string.IsNullOrWhiteSpace(url))
            {
                Apps.Add(url);
            }
        }

        private void OnAppSelected(AppInfo appInfo)
        {
            string app = appInfo?.Title;
            if (!string.IsNullOrWhiteSpace(app))
            {
                Urls.Add(app);
            }
        }

        private void AppsOrUrlsCollectonChanges()
        {
            HasChanges = true;
            this.RaisePropertyChanged(nameof(AppsAndUrls));
        }
    }
}
