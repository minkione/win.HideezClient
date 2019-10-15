using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
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
using HideezClient.Mvvm;
using HideezClient.Utilities;
using HideezClient.Utilities.QrCode;
using MvvmExtensions.Commands;
using NLog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Unity;

namespace HideezClient.ViewModels
{
    class EditAccountViewModel : ReactiveObject
    {
        protected readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly IQrScannerHelper qrScannerHelper;
        private readonly IWindowsManager windowsManager;
        private bool isUpdateAppsUrls;
        private DeviceViewModel device;
        private int generatePasswordLength = 16;
        private readonly AppInfo loadingAppInfo = new AppInfo { Description = "Loading...", Domain = "Loading..." };
        private readonly AppInfo addUrlAppInfo = new AppInfo { Domain = "<Enter Url>" };
        private bool canScanOtpSecretQrCode = true;
        private bool cacheHasOtp;

        public EditAccountViewModel(DeviceViewModel device, IWindowsManager windowsManager, IQrScannerHelper qrScannerHelper)
            :this(device, null, windowsManager, qrScannerHelper)
        { }

        public EditAccountViewModel(DeviceViewModel device, AccountRecord accountRecord, IWindowsManager windowsManager, IQrScannerHelper qrScannerHelper)
        {
            this.windowsManager = windowsManager;
            this.qrScannerHelper = qrScannerHelper;
            this.device = device;

            if (accountRecord == null)
            {
                IsNewAccount = true;
                AccountRecord = new AccountRecord();
            }
            else
            {
                AccountRecord = new AccountRecord
                {
                    Key = accountRecord.Key,
                    Flags = accountRecord.Flags,
                    Name = accountRecord.Name,
                    Login = accountRecord.Login,
                    Password = accountRecord.Password,
                    OtpSecret = accountRecord.OtpSecret,
                    Apps = accountRecord.Apps,
                    Urls = accountRecord.Urls,
                    IsPrimary = accountRecord.IsPrimary,
                };
                InitProp(AccountRecord);
            }
            InitDependencies();
        }

        private void InitDependencies()
        {
            Application.Current.MainWindow.Activated += WeakEventHandler.Create(this, (@this, o, args) => Task.Run(@this.UpdateAppsAndUrls));

            this.WhenAnyValue(vm => vm.SelectedApp).Subscribe(OnAppSelected);
            this.WhenAnyValue(vm => vm.SelectedUrl).Subscribe(OnUrlSelected);

            Observable.FromEventPattern<NotifyCollectionChangedEventArgs>(AppsAndUrls, nameof(ObservableCollection<string>.CollectionChanged))
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

            cacheHasOtp = HasOpt;
            AccountRecord.OtpSecret = "";
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
            if (accountRecord.Apps != null)
            {
                AppsAndUrls.AddRange(AccountUtility.Split(accountRecord.Apps).Select(u => new AppViewModel(u)));
            }
            if (accountRecord.Urls != null)
            {
                AppsAndUrls.AddRange(AccountUtility.Split(accountRecord.Urls).Select(u => new AppViewModel(u, true)));
            }
        }

        #region Propertys

        [Reactive] public bool IsNewAccount { get; private set; }
        public string Name
        {
            get { return AccountRecord.Name; }
            set
            {
                if (AccountRecord.Name != value)
                {
                    AccountRecord.Name = value;
                    this.RaisePropertyChanged(nameof(Name));
                }
            }
        }
        public string Password
        {
            get { return AccountRecord.Password; }
            set
            {
                if (AccountRecord.Password != value)
                {
                    AccountRecord.Password = value;
                    this.RaisePropertyChanged(nameof(Password));
                }
            }
        }
        public string Login
        {
            get { return AccountRecord.Login; }
            set
            {
                if (AccountRecord.Login != value)
                {
                    AccountRecord.Login = value;
                    this.RaisePropertyChanged(nameof(Login));
                }
            }
        }

        #region OTP
        [Reactive] public bool EditOtp { get; set; }
        public bool HasOpt
        {
            get
            {
                return AccountRecord.HasOtp;
            }
            set
            {
                if (AccountRecord.HasOtp != value)
                {
                    if (value)
                    {
                        AccountRecord.Flags = (ushort)(AccountRecord.Flags | (ushort)Hideez.SDK.Communication.StorageTableFlags.HAS_OTP);
                    }
                    else
                    {
                        AccountRecord.Flags = (ushort)(AccountRecord.Flags & ~(ushort)Hideez.SDK.Communication.StorageTableFlags.HAS_OTP);
                    }

                    this.RaisePropertyChanged(nameof(HasOpt));
                }
            }
        }
        public string OtpSecret
        {
            get { return AccountRecord.OtpSecret; }
            set
            {
                if (AccountRecord.OtpSecret != value)
                {
                    var secret = value.Replace(" ", "");
                    if (!string.IsNullOrEmpty(secret))
                    {
                        EditOtp = true;
                        if (ValidateBase32String(secret))
                        {
                            AccountRecord.OtpSecret = secret;
                            ErrorOtpSecret = null;
                        }
                        else
                        {
                            ErrorOtpSecret = "Not valid OTP secret";
                            AccountRecord.OtpSecret = value;
                        }
                    }
                    else
                    {
                        HasOpt = false;
                        AccountRecord.OtpSecret = "";
                        ErrorOtpSecret = null;
                    }
                    this.RaisePropertyChanged(nameof(OtpSecret));
                }
            }
        }
        [Reactive] public string ErrorOtpSecret { get; set; }

        #endregion OTP

        public bool IsPrimary
        {
            get { return AccountRecord.IsPrimary; }
            set
            {
                if (AccountRecord.IsPrimary != value)
                {
                    AccountRecord.IsPrimary = value;
                    this.RaisePropertyChanged(nameof(IsPrimary));
                }
            }
        }

        public IEnumerable<string> Apps { get { return AppsAndUrls.Where(x => !x.IsUrl).Select(x => x.Title); } }
        public IEnumerable<string> Urls { get { return AppsAndUrls.Where(x => x.IsUrl).Select(x => x.Title); } }
        public ObservableCollection<AppViewModel> AppsAndUrls { get; } = new ObservableCollection<AppViewModel>();
        public IEnumerable<string> Logins { get { return device?.Accounts.Select(a => a.Login).Distinct(); } }
        public ObservableCollection<AppInfo> OpenedApps { get; } = new ObservableCollection<AppInfo>();
        public ObservableCollection<AppInfo> OpenedForegroundUrls { get; } = new ObservableCollection<AppInfo>();

        [Reactive] public AppInfo SelectedApp { get; set; }
        [Reactive] public AppInfo SelectedUrl { get; set; }

        public AccountRecord AccountRecord { get; }

        #endregion Propertys

        #region Command

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

        [Reactive] public ICommand DeleteAccountCommand { get; set; }
        [Reactive] public ICommand CancelCommand { get; set; }
        [Reactive] public ICommand SaveAccountCommand { get; set; }

        #region OTP

        public ICommand ScanOtpSecretFromQRCodeCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        OnScanOtpSecretFromQRCode();
                    },
                    CanExecuteFunc = () => canScanOtpSecretQrCode,
                };
            }
        }

        public ICommand EnterOtpCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        EditOtp = true;
                    },
                };
            }
        }

        public ICommand EditOtpCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        EditOtp = true;
                    },
                };
            }
        }

        public ICommand DeleteOtpCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        HasOpt = false;
                        OtpSecret = "";
                        EditOtp = false;
                    },
                };
            }
        }

        public ICommand CancelEditOtpCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = (x) =>
                    {
                        OtpSecret = "";
                        EditOtp = false;
                        HasOpt = cacheHasOtp;
                    },
                };
            }
        }

        #endregion

        #endregion

        private bool ValidateBase32String(string base32)
        {
            return Base32Encoding.Validate(base32);
        }

        private void OnRemoveAppInfo(AppViewModel appViewModel)
        {
            if (appViewModel != null)
            {
                AppsAndUrls.Remove(appViewModel);
            }
        }

        private async void OnScanOtpSecretFromQRCode()
        {
            canScanOtpSecretQrCode = false;
            bool isScanedQr = false;

            try
            {
                var screenShot = await windowsManager.GetCurrentScreenImageAsync();
                var scanResult = qrScannerHelper.DecoreQrFromImage(screenShot);

                if (!string.IsNullOrWhiteSpace(scanResult?.Text))
                {
                    OtpSecret = qrScannerHelper.GetOtpSecret(scanResult.Text);
                    isScanedQr = true;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            if (!isScanedQr)
            {
                windowsManager.ShowWarn(LocalizedObject.L("Notify.NotScanQr"));
            }

            canScanOtpSecretQrCode = true;
        }

        private string OnGeneratePassword()
        {
            string password = PasswordGenerator.Generate(generatePasswordLength);
            return password;
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
            AccountRecord.Apps = AccountUtility.JoinAppsOrUrls(Apps);
            AccountRecord.Urls = AccountUtility.JoinAppsOrUrls(Urls);
            this.RaisePropertyChanged(nameof(Urls));
            this.RaisePropertyChanged(nameof(Apps));
        }
    }
}
