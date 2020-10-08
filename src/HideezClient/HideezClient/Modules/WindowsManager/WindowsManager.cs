using HideezClient.Mvvm;
using HideezClient.ViewModels;
using HideezClient.Views;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Windows.Threading;
using HideezClient.Models;
using HideezClient.Controls;
using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using HideezClient.Utilities;
using System.Windows.Interop;
using HideezClient.Dialogs;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using HideezMiddleware.Settings;
using HideezClient.Models.Settings;
using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using HideezClient.Modules.NotificationsManager;
using Meta.Lib.Modules.PubSub;
using HideezMiddleware.IPC.Messages;

namespace HideezClient.Modules
{
    class WindowsManager : IWindowsManager
    {
        private readonly ViewModelLocator _viewModelLocator;
        private string titleNotification;
        private readonly Logger log = LogManager.GetCurrentClassLogger(nameof(WindowsManager));
        private bool isMainWindowVisible;
        private readonly ISettingsManager<ApplicationSettings> _settingsManager;
        readonly INotificationsManager _notificationsManager;

        readonly object pinDialogLock = new object();
        PinDialog pinView = null;

        readonly object activationDialogLock = new object();
        ActivationDialog activationView = null;

        bool _initialized = false;
        int _mainWindowActivationInterlock = 0;

        public event EventHandler<bool> MainWindowVisibleChanged;

        public WindowsManager(ViewModelLocator viewModelLocator, INotificationsManager notificationsManager,
             ISettingsManager<ApplicationSettings> settingsManager, IMetaPubSub metaMessenger)
        {
            _viewModelLocator = viewModelLocator;
            _settingsManager = settingsManager;
            _notificationsManager = notificationsManager;

            metaMessenger.Subscribe<ShowWarningNotificationMessage>(ShowWarn);
            metaMessenger.Subscribe<ShowInfoNotificationMessage>(ShowInfo);
            metaMessenger.Subscribe<ShowErrorNotificationMessage>(ShowError);
            metaMessenger.Subscribe<ShowLockNotificationMessage>(ShowLockNotification);
            metaMessenger.Subscribe<ShowLowBatteryNotificationMessage>(ShowLowBatteryNotification);

            metaMessenger.TrySubscribeOnServer<WorkstationUnlockedMessage>(ClearNotifications);

            metaMessenger.TrySubscribeOnServer<UserNotificationMessage>((m)=>ShowInfo(new ShowInfoNotificationMessage(m.Message, notificationId:m.NotificationId)));
            metaMessenger.TrySubscribeOnServer<UserErrorMessage>((m) => ShowError(new ShowErrorNotificationMessage(m.Message, notificationId: m.NotificationId)));
            //messenger.Register<ServiceNotificationReceivedMessage>(this, (p) => ShowInfo(p.Message, notificationId: p.Id));
            //messenger.Register<ServiceErrorReceivedMessage>(this, (p) => ShowError(p.Message, notificationId: p.Id));

            metaMessenger.Subscribe<ShowButtonConfirmUiMessage>(ShowButtonConfirmAsync);
            metaMessenger.Subscribe<ShowPinUiMessage>(ShowPinAsync);
            metaMessenger.Subscribe<HidePinUiMessage>(HidePinAsync);
            metaMessenger.TrySubscribeOnServer<ShowActivationCodeUiMessage>(ShowActivationDialogAsync);
            metaMessenger.TrySubscribeOnServer<HideActivationCodeUi>(HideActivationDialogAsync);

            metaMessenger.Subscribe<ShowActivateMainWindowMessage>((p) => ActivateMainWindow());
        }
        
        public Task ActivateMainWindow()
        {
            UIDispatcher.Invoke(OnActivateMainWindow);
            return Task.CompletedTask;
        }

        public void HideMainWindow()
        {
            UIDispatcher.Invoke(OnHideMainWindow);
        }

        public void InitializeMainWindow()
        {
            UIDispatcher.Invoke(OnInitializeMainWindow);
        }

        public async Task ActivateMainWindowAsync()
        {
            await UIDispatcher.InvokeAsync(OnActivateMainWindow);
        }

        public async Task HideMainWindowAsync()
        {
            await UIDispatcher.InvokeAsync(OnHideMainWindow);
        }

        public async Task InitializeMainWindowAsync()
        {
            await UIDispatcher.InvokeAsync(OnInitializeMainWindow);
        }

        public bool IsMainWindowVisible
        {
            get { return isMainWindowVisible; }
            private set
            {
                if (isMainWindowVisible != value)
                {
                    isMainWindowVisible = value;
                    OnMainWindowVisibleChanged(isMainWindowVisible);
                }
            }
        }

        private Window MainWindow { get { return Application.Current.MainWindow; } }

        private Dispatcher UIDispatcher { get { return Application.Current.Dispatcher; } }

        private void OnActivateMainWindow()
        {
            if (Interlocked.CompareExchange(ref _mainWindowActivationInterlock, 1, 0) == 0)
            {
                try
                {
                    if (MainWindow == null || !_initialized) return;

                    // event is only subscribed to once
                    UnsubscribeToMainWindowEvent();
                    SubscribeToMainWindowEvent();

                    MainWindow.Show();

                    if (MainWindow.WindowState == WindowState.Minimized)
                    {
                        MainWindow.WindowState = WindowState.Normal;
                    }

                    MainWindow.Activate();
                    MainWindow.Topmost = true;
                    MainWindow.Topmost = false;
                    MainWindow.Focus();
                }
                finally
                {
                    Interlocked.Exchange(ref _mainWindowActivationInterlock, 0);
                }
            }
        }

        private void OnHideMainWindow()
        {
            if (MainWindow == null || !_initialized) return;

            // event is only subscribed to once
            UnsubscribeToMainWindowEvent();
            SubscribeToMainWindowEvent();

            if (MainWindow.WindowState == WindowState.Normal)
            {
                MainWindow.WindowState = WindowState.Minimized;
            }

            MainWindow.Hide();
        }

        private void DisposeMainWindow()
        {
            if (MainWindow == null) return;

            UnsubscribeToMainWindowEvent();

            MainWindow.Hide();
            MainWindow.Close();
            Application.Current.MainWindow = null;
        }

        private void OnInitializeMainWindow()
        {
            DisposeMainWindow();

            if (_settingsManager.Settings.UseSimplifiedUI)
                Application.Current.MainWindow = new SimpleMainView();
            else
                Application.Current.MainWindow = new MainWindowView();

            _initialized = true;
        }

        private void SubscribeToMainWindowEvent()
        {
            MainWindow.IsVisibleChanged += MainWindow_IsVisibleChanged;
        }

        private void UnsubscribeToMainWindowEvent()
        {
            MainWindow.IsVisibleChanged -= MainWindow_IsVisibleChanged;
        }

        private void MainWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IsMainWindowVisible = MainWindow.IsVisible;
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
            IsMainWindowVisible = false;
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            IsMainWindowVisible = true;
        }

        private void OnMainWindowVisibleChanged(bool isVisivle)
        {
            try
            {
                MainWindowVisibleChanged?.Invoke(this, isVisivle);
                log.WriteLine($"Main window is visible changed: {isVisivle}");
            }
            catch (Exception ex)
            {
                log.WriteLine(ex);
            }
        }

        private Task ShowLockNotification(ShowLockNotificationMessage message)
        {
            UIDispatcher.Invoke(() => _notificationsManager.ShowNotification(message.NotificationId, message.Title ?? GetTitle(), message.Message, NotificationIconType.Lock, message.Options));
            return Task.CompletedTask;
        }

        private Task ShowError(ShowErrorNotificationMessage message)
        {
            UIDispatcher.Invoke(() => _notificationsManager.ShowNotification(message.NotificationId, message.Title ?? GetTitle(), message.Message, NotificationIconType.Error, message.Options));
            return Task.CompletedTask;
        }

        private Task ShowWarn(ShowWarningNotificationMessage message)
        {
            UIDispatcher.Invoke(() => _notificationsManager.ShowNotification(message.NotificationId, message.Title ?? GetTitle(), message.Message, NotificationIconType.Warn, message.Options));
            return Task.CompletedTask;
        }

        private Task ShowInfo(ShowInfoNotificationMessage message)
        {
            UIDispatcher.Invoke(() => _notificationsManager.ShowNotification(message.NotificationId, message.Title ?? GetTitle(), message.Message, NotificationIconType.Info, message.Options));
            return Task.CompletedTask;
        }

        private Task ShowLowBatteryNotification(ShowLowBatteryNotificationMessage message)
        {
            UIDispatcher.Invoke(() => _notificationsManager.ShowNotification(message.NotificationId, message.Title ?? GetTitle(), message.Message, NotificationIconType.Warn, message.Options));
            return Task.CompletedTask;
        }

        private Task ClearNotifications(WorkstationUnlockedMessage message)
        {
            UIDispatcher.Invoke(() => _notificationsManager.ClearNotifications());
            return Task.CompletedTask;
        }

        public Task<bool> ShowAccountNotFoundAsync(string message, string title = null)
        {
            return UIDispatcher.Invoke(() => _notificationsManager.ShowAccountNotFoundNotification(title ?? GetTitle(), message));
        }

        private string GetTitle()
        {
            if (titleNotification == null)
            {
                // Commented, because assembly name does not contain a space between two words and 
                // assembly name will not be changed just for notification titles
                //titleNotification = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}";
                titleNotification = "Hideez Client";
            }

            return titleNotification;
        }

        public Task<Account> SelectAccountAsync(Account[] accounts, IntPtr hwnd)
        {
            return UIDispatcher.Invoke(() => _notificationsManager.SelectAccountAsync(accounts, hwnd));
        }

        public void ShowCredentialsLoading(CredentialsLoadNotificationViewModel viewModel)
        {
            UIDispatcher.Invoke(() => _notificationsManager.ShowStorageLoadingNotification(viewModel));
        }

        public void CloseWindow(string id)
        {
            UIDispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext is IRequireViewIdentification vm && vm.ObservableId == id)
                    {
                        window.Close();
                    }
                }
            });
        }

        private void SetStartupLocation(Window window, bool mainWindowWasOpen, bool hideMainWindow = false)
        {
            if (mainWindowWasOpen)
            {
                window.Owner = MainWindow;
                if (hideMainWindow)
                {
                    MainWindow?.Hide();
                }
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        Task ShowButtonConfirmAsync(ShowButtonConfirmUiMessage message)
        {
            lock (pinDialogLock)
            {
                if (pinView == null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        if (MainWindow is MetroWindow metroWindow)
                        {
                            var vm = _viewModelLocator.PinViewModel;
                            vm.Initialize(message.DeviceId);
                            pinView = new PinDialog(vm);
                            pinView.Closed += PinView_Closed;
                            OnActivateMainWindow();
                            metroWindow.ShowMetroDialogAsync(pinView);
                        }
                    });
                }

                if (pinView != null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        ((PinViewModel)pinView.DataContext).UpdateViewModel(message.DeviceId, true, false, false);
                    });
                }
            }

            return Task.CompletedTask;
        }

        Task ShowPinAsync(ShowPinUiMessage message)
        {
            lock (pinDialogLock)
            {
                if (pinView == null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        if (MainWindow is MetroWindow metroWindow)
                        {
                            var vm = _viewModelLocator.PinViewModel;
                            vm.Initialize(message.DeviceId);
                            pinView = new PinDialog(vm);
                            pinView.Closed += PinView_Closed;
                            OnActivateMainWindow();
                            metroWindow.ShowMetroDialogAsync(pinView);
                        }
                    });
                }

                if (pinView != null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        ((PinViewModel)pinView.DataContext).UpdateViewModel(message.DeviceId, false, message.OldPin, message.ConfirmPin);
                    });
                }
            }
            return Task.CompletedTask;
        }

        Task HidePinAsync(HidePinUiMessage message)
        {
            try
            {
                UIDispatcher.Invoke(() =>
                {
                    try
                    {
                        pinView?.Close();
                        pinView = null;
                    }
                    catch { }
                });
            }
            catch { }

            return Task.CompletedTask;
        }

        Task ShowActivationDialogAsync(HideezMiddleware.IPC.Messages.ShowActivationCodeUiMessage obj)
        {
            try
            {
                if (activationView == null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        if (MainWindow is MetroWindow metroWindow)
                        {
                            var vm = _viewModelLocator.ActivationViewModel;
                            vm.Initialize(obj.DeviceId);
                            activationView = new ActivationDialog(vm);
                            activationView.Closed += ActivationView_Closed;
                            OnActivateMainWindow();
                            metroWindow.ShowMetroDialogAsync(activationView);
                        }
                    });
                }

                if (activationView != null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        ((ActivationViewModel)activationView.DataContext).UpdateViewModel(obj.DeviceId);
                    });
                }
            }
            catch { }

            return Task.CompletedTask;
        }

        Task HideActivationDialogAsync(HideActivationCodeUi message)
        {
            try
            {
                UIDispatcher.Invoke(() =>
                {
                    try
                    {
                        activationView?.Close();
                        activationView = null;
                    }
                    catch { }
                });
            }
            catch { }

            return Task.CompletedTask;
        }

        void PinView_Closed(object sender, EventArgs e)
        {
            lock (pinDialogLock)
            {
                if (pinView != null)
                {
                    pinView.Closed -= PinView_Closed;
                    pinView = null;
                }
            }
        }

        void ActivationView_Closed(object sender, EventArgs e)
        {
            lock (activationDialogLock)
            {
                if (activationView != null)
                {
                    activationView.Closed -= ActivationView_Closed;
                    activationView = null;
                }
            }
        }

        public async Task<Bitmap> GetCurrentScreenImageAsync()
        {
            Bitmap screenShot = new Bitmap(1, 1);
            System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.PrimaryScreen;
            var isMainWindowVisible = false;

            try
            {
                var hWndForegroundWindow = Win32Helper.GetForegroundWindow();
                screen = System.Windows.Forms.Screen.FromHandle(hWndForegroundWindow);

                if (MainWindow != null)
                {
                    IntPtr hWndMainWindow = new WindowInteropHelper(MainWindow).EnsureHandle();
                    isMainWindowVisible = hWndForegroundWindow == hWndMainWindow;
                }

                if (isMainWindowVisible)
                {
                    await HideMainWindowAsync();
                }

                screenShot = GetCurrentScreenImage(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);

                if (isMainWindowVisible)
                {
                    await ActivateMainWindowAsync();
                }

            }
            catch (Exception ex)
            {
                log.WriteLine(ex);
            }

            return screenShot;
        }

        public Bitmap GetCurrentScreenImage(double sourceX, double sourceY, double width, double height)
        {

            Bitmap bitmap = new Bitmap((int)width, (int)height);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen((int)sourceX, (int)sourceY, 0, 0, bitmap.Size);
            }

            return bitmap;
        }

        public Task<bool> ShowDeleteCredentialsPromptAsync()
        {
            var vm = new MessageViewModel();
            vm.SetCaptionFormat("MessageBox.DeleteCredentials.Caption");
            vm.SetMessageFormat("MessageBox.DeleteCredentials.Message");
            return ShowMessageViewAsync(vm, "WarnIco", "Button.YesDelete", "Button.Cancel");
        }

        public Task<bool> ShowDisconnectDevicePromptAsync(string deviceName)
        {
            var vm = new MessageViewModel();
            vm.SetCaptionFormat("MessageBox.DisconectDevice.Caption", deviceName);
            vm.SetMessageFormat("MessageBox.DisconectDevice.Message", deviceName);
            return ShowMessageViewAsync(vm, "WarnIco", "Button.Yes", "Button.No");
        }

        public Task<bool> ShowRemoveDevicePromptAsync(string deviceName)
        {
            var vm = new MessageViewModel();
            vm.SetCaptionFormat("MessageBox.DeleteDevice.Caption", deviceName);
            vm.SetMessageFormat("MessageBox.DeleteDevice.Message", deviceName);
            return ShowMessageViewAsync(vm, "WarnIco", "Button.Yes", "Button.No");
        }

        private Task<bool> ShowMessageViewAsync(MessageViewModel viewModel, string icoKey, string confirmButtonTextKey = "Button.Ok", string cancelButtonTextKey = "")
        {
            viewModel.Tcs = new TaskCompletionSource<bool>();

            UIDispatcher.InvokeAsync(() =>
            {
                var messageBox = new Dialogs.MessageDialog(icoKey, confirmButtonTextKey, cancelButtonTextKey)
                {
                    DataContext = viewModel
                };
                if (MainWindow is MetroWindow metroWindow)
                {
                    metroWindow.ShowMetroDialogAsync(messageBox);
                }
            });

            return viewModel.Tcs.Task;
        }
    }
}

