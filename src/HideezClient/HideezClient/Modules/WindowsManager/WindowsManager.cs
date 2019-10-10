using HideezClient.Modules.ActionHandler;
using HideezClient.Mvvm;
using HideezClient.ViewModels;
using HideezClient.Views;
using NLog;
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

namespace HideezClient.Modules
{
    class WindowsManager : IWindowsManager
    {
        private readonly ViewModelLocator _viewModelLocator;
        private string titleNotification;
        private readonly INotifier _notifier;
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private bool isMainWindowVisible;

        object pinWindowLock = new object();
        PinView pinView = null;

        public event EventHandler<bool> MainWindowVisibleChanged;

        public WindowsManager(INotifier notifier, ViewModelLocator viewModelLocator, IMessenger messenger)
        {
            _notifier = notifier;
            _viewModelLocator = viewModelLocator;

            messenger.Register<ServiceNotificationReceivedMessage>(this, (p) => ShowInfo(p.Message, notificationId: p.Id));
            messenger.Register<ServiceErrorReceivedMessage>(this, (p) => ShowError(p.Message, notificationId: p.Id));

            messenger.Register<ShowInfoNotificationMessage>(this, (p) => ShowInfo(p.Message, p.Title, notificationId: p.NotificationId));
            messenger.Register<ShowWarningNotificationMessage>(this, (p) => ShowWarn(p.Message, p.Title, notificationId: p.NotificationId));
            messenger.Register<ShowErrorNotificationMessage>(this, (p) => ShowError(p.Message, p.Title, notificationId: p.NotificationId));

            messenger.Register<ShowButtonConfirmUiMessage>(this, ShowButtonConfirmAsync);
            messenger.Register<ShowPinUiMessage>(this, ShowPinAsync);
            messenger.Register<HidePinUiMessage>(this, HidePinAsync);
        }

        public void ActivateMainWindow()
        {
            UIDispatcher.Invoke(OnActivateMainWindow);
        }

        public async Task ActivateMainWindowAsync()
        {
            await UIDispatcher.InvokeAsync(OnActivateMainWindow);
        }

        public async Task HideMainWindowAsync()
        {
            await UIDispatcher.InvokeAsync(OnHideMainWindow);
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
            if (MainWindow == null) return;

            // event is only subscribed to once
            UnsubscribeToMainWindowEvent();
            SubscribeToMainWindowEvent();

            if (!MainWindow.IsVisible)
            {
                MainWindow.Show();
            }

            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.WindowState = WindowState.Normal;
            }

            MainWindow.Activate();
            MainWindow.Topmost = true;
            MainWindow.Topmost = false;
            MainWindow.Focus();
        }

        private void OnHideMainWindow()
        {
            if (MainWindow == null) return;

            // event is only subscribed to once
            UnsubscribeToMainWindowEvent();
            SubscribeToMainWindowEvent();

            if (MainWindow.WindowState == WindowState.Normal)
            {
                MainWindow.WindowState = WindowState.Minimized;
            }

            MainWindow.Hide();
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
                log.Info($"Main window is visible changed: {isVisivle}");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        public void ShowDialogAddCredential(Device device)
        {
            UIDispatcher.Invoke(() =>
            {
                var addCredentialWindow = new AddCredentialView();
                SetStartupLocation(addCredentialWindow, IsMainWindowVisible);
                if (addCredentialWindow.DataContext is AddCredentialViewModel viewModel)
                {
                    viewModel.Device = device;
                }
                addCredentialWindow.ShowDialog();
            });
        }

        public void ShowError(string message, string title = null, string notificationId = "")
        {
            UIDispatcher.Invoke(() => _notifier.ShowError(notificationId, title ?? GetTitle(), message));
        }

        public void ShowWarn(string message, string title = null, string notificationId = "")
        {
            UIDispatcher.Invoke(() => _notifier.ShowWarn(notificationId, title ?? GetTitle(), message));
        }

        public void ShowInfo(string message, string title = null, string notificationId = "")
        {
            UIDispatcher.Invoke(() => _notifier.ShowInfo(notificationId, title ?? GetTitle(), message));
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
            return UIDispatcher.Invoke(() => _notifier.SelectAccountAsync(accounts, hwnd));
        }

        public void ShowCredentialsLoading(CredentialsLoadNotificationViewModel viewModel)
        {
            UIDispatcher.Invoke(() => _notifier.ShowStorageLoadingNotification(viewModel));
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

        void ShowButtonConfirmAsync(ShowButtonConfirmUiMessage obj)
        {
            lock (pinWindowLock)
            {
                if (pinView == null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        var vm = _viewModelLocator.PinViewModel;
                        vm.Initialize(obj.DeviceId);
                        pinView = new PinView()
                        {
                            DataContext = vm,
                        };
                        pinView.Closed += PinView_Closed;
                        pinView.Show();
                    });
                }

                if (pinView != null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        ((PinViewModel)pinView.DataContext).UpdateViewModel(obj.DeviceId, true, false, false);
                    });
                }
            }
        }

        void ShowPinAsync(ShowPinUiMessage obj)
        {
            lock (pinWindowLock)
            {
                if (pinView == null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        var vm = _viewModelLocator.PinViewModel;
                        vm.Initialize(obj.DeviceId);
                        pinView = new PinView()
                        {
                            DataContext = vm,
                        };
                        pinView.Closed += PinView_Closed;
                        pinView.Show();
                    });
                }

                if (pinView != null)
                {
                    UIDispatcher.Invoke(() =>
                    {
                        ((PinViewModel)pinView.DataContext).UpdateViewModel(obj.DeviceId, false, obj.OldPin, obj.ConfirmPin);
                    });
                }
            }
        }

        void HidePinAsync(HidePinUiMessage obj)
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
        }

        void PinView_Closed(object sender, EventArgs e)
        {
            lock (pinWindowLock)
            {
                if (pinView != null)
                {
                    pinView.Closed -= PinView_Closed;
                    pinView = null;
                }
            }
        }


        public Task ShowDeviceLockedAsync()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            UIDispatcher.InvokeAsync(() =>
            {
                DeviceLockedView dlv = new DeviceLockedView();
                SetStartupLocation(dlv, IsMainWindowVisible);
                dlv.Closed += (sender, e) => tcs.TrySetResult(true);
                dlv.Show();
            });

            return tcs.Task;
        }

        public void ShowDeviceNotAuthorized(Device device)
        {
            UIDispatcher.Invoke(() => _notifier.ShowDeviceNotAuthorizedNotification(device));
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
                log.Error(ex);
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
    }
}

