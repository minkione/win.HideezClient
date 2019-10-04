using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private int generatePasswordLength = 16;
        private readonly AppInfo loadingAppInfo = new AppInfo { Description = "Loading...", Domain = "Loading..." };
        private readonly AppInfo addUrlAppInfo = new AppInfo { Domain = "<Enter Url>" };

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

            this.WhenAnyValue(vm => vm.Name, vm => vm.Login, vm => vm.HasOpt, vm => vm.OtpSecret)
                .Where(_ => IsEditable)
                .Subscribe(_ => HasChanges = true);

            this.WhenAnyValue(vm => vm.SelectedApp).Subscribe(OnAppSelected);
            this.WhenAnyValue(vm => vm.SelectedUrl).Subscribe(OnUrlSelected);

            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(AppsAndUrls, nameof(ObservableCollection<string>.CollectionChanged))
                       .Where(_ => IsEditable)
                      .Subscribe(change => AppsOrUrlsCollectonChanges());

            OpenedApps.Add(loadingAppInfo);
            OpenedForegroundUrls.Add(loadingAppInfo);
            OpenedForegroundUrls.Add(addUrlAppInfo);
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
                AppsAndUrls.AddRange(AccountUtility.Split(accountRecord.Apps).Select(u => new AppViewModel(u)));
            }
            if (accountRecord.Urls != null)
            {
                AppsAndUrls.AddRange(AccountUtility.Split(accountRecord.Urls).Select(u => new AppViewModel(u, true)));
            }
        }

        [Reactive] public bool IsEditable { get; set; } = true;
        [Reactive] public bool HasChanges { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public string Login { get; set; }
        [Reactive] public bool HasOpt { get; protected set; }
        [Reactive] public string OtpSecret { get; set; }

        public IEnumerable<string> Apps { get { return AppsAndUrls.Where(x => !x.IsUrl).Select(x => x.Title); } }
        public IEnumerable<string> Urls { get { return AppsAndUrls.Where(x => x.IsUrl).Select(x => x.Title); } }
        public ObservableCollection<AppViewModel> AppsAndUrls { get; } = new ObservableCollection<AppViewModel>();
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
                        if (x is PasswordBox passwordBox)
                        {
                            string password = OnGeneratePassword();
                            passwordBox.Password = password;
                        }
                    }
                };
            }
        }

        public ICommand ScanOtpSecretFromQRCodeCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        OnScanOtpSecretFromQRCode();
                    }
                };
            }
        }

        public ICommand RemoveAppInfoCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        OnRemoveAppInfo(x as AppViewModel);
                    }
                };
            }
        }

        public ICommand ApplyAppInfoCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        if (x is AppViewModel viewModel)
                        {
                            viewModel.ApplyChanges();
                        }
                    },
                };
            }
        }

        public ICommand EditAppInfoCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        if (x is AppViewModel viewModel)
                        {
                            RemoveEmpty();
                            viewModel.Edit();
                        }
                    },
                };
            }
        }

        public ICommand CancelAppInfoCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        if (x is AppViewModel viewModel)
                        {
                            viewModel.CancelEdit();
                            RemoveEmpty();
                        }
                    },
                };
            }
        }

        #endregion

        private void OnRemoveAppInfo(AppViewModel appViewModel)
        {
            if (appViewModel != null)
            {
                AppsAndUrls.Remove(appViewModel);
            }
        }

        private void OnScanOtpSecretFromQRCode()
        {
            OtpSecret = "scaned otp secret";
        }

        private string OnGeneratePassword()
        {
            string password = PasswordGenerator.Generate(generatePasswordLength);
            return password;
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

        private void RemoveEmpty()
        {
            foreach (var item in AppsAndUrls.Where(x => string.IsNullOrWhiteSpace(x.Title)).ToArray())
            {
                AppsAndUrls.Remove(item);
            }
        }

        private void OnUrlSelected(AppInfo appInfo)
        {
            if (appInfo == null) return;

            RemoveEmpty();
            if (appInfo == addUrlAppInfo)
            {
                var newCustomUrl = new AppViewModel("", true) { IsInEditState = true, };
                AppsAndUrls.Add(newCustomUrl);
            }
            else if (appInfo != loadingAppInfo)
            {
                string url = appInfo?.Domain;
                if (!string.IsNullOrWhiteSpace(url) && AppsAndUrls.FirstOrDefault(x => x.Title == url) == null)
                {
                    AppsAndUrls.Add(new AppViewModel(url, true));
                }
            }

            SelectedUrl = null;
        }

        private void OnAppSelected(AppInfo appInfo)
        {
            if (appInfo == null) return;

            RemoveEmpty();

            if (appInfo != loadingAppInfo)
            {
                string app = appInfo?.Title;
                if (!string.IsNullOrWhiteSpace(app) && AppsAndUrls.FirstOrDefault(x => x.Title == app) == null)
                {
                    AppsAndUrls.Add(new AppViewModel(app));
                }
            }
            SelectedApp = null;
        }

        private void AppsOrUrlsCollectonChanges()
        {
            HasChanges = true;
            this.RaisePropertyChanged(nameof(Urls));
            this.RaisePropertyChanged(nameof(Apps));
        }
    }
}
