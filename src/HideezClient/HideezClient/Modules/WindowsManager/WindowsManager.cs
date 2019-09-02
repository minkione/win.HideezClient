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

namespace HideezClient.Modules
{
    class WindowsManager : IWindowsManager
    {
        private readonly ViewModelLocator viewModelLocator;
        private string titleNotification;
        private readonly INotifier notifier;
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private bool isMainWindowVisible;

        public event EventHandler<bool> MainWindowVisibleChanged;

        public WindowsManager(INotifier notifier, ViewModelLocator viewModelLocator)
        {
            this.notifier = notifier;
            this.viewModelLocator = viewModelLocator;
        }

        public void ActivateMainWindow()
        {
            if (UIDispatcher.CheckAccess())
            {
                OnActivateMainWindow();
            }
            else
            {
                // Do non UI Thread stuff
                UIDispatcher.Invoke(OnActivateMainWindow);
            }
        }

        public async Task ActivateMainWindowAsync()
        {
            if (UIDispatcher.CheckAccess())
            {
                await Task.Run(new Action(OnActivateMainWindow));
            }
            else
            {
                // Do non UI Thread stuff
                await UIDispatcher.InvokeAsync(OnActivateMainWindow);
            }
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

            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.WindowState = WindowState.Normal;
            }

            MainWindow.Show();
            MainWindow.Activate();
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

        public void ShowError(string message, string title = null)
        {
            if (UIDispatcher.CheckAccess())
            {
                notifier.ShowError(title ?? GetTitle(), message);
            }
            else
            {
                // Do non UI Thread stuff
                UIDispatcher.Invoke(() => notifier.ShowError(title ?? GetTitle(), message));
            }
            // MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowWarn(string message, string title = null)
        {
            if (UIDispatcher.CheckAccess())
            {
                notifier.ShowWarn(title ?? GetTitle(), message);
            }
            else
            {
                // Do non UI Thread stuff
                UIDispatcher.Invoke(() => notifier.ShowWarn(title ?? GetTitle(), message));
            }
            // MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public void ShowInfo(string message, string title = null)
        {
            if (UIDispatcher.CheckAccess())
            {
                notifier.ShowInfo(title ?? GetTitle(), message);
            }
            else
            {
                // Do non UI Thread stuff
                UIDispatcher.Invoke(() => notifier.ShowInfo(title ?? GetTitle(), message));
            }
            // MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GetTitle()
        {
            if (titleNotification == null)
            {
                titleNotification = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}";
            }

            return titleNotification;
        }

        public Task<Account> SelectAccountAsync(Account[] accounts, IntPtr hwnd)
        {
            if (UIDispatcher.CheckAccess())
            {
                return notifier.SelectAccountAsync(accounts, hwnd);
            }
            else
            {
                // Do non UI Thread stuff
                return UIDispatcher.Invoke(() => notifier.SelectAccountAsync(accounts, hwnd));
            }
        }

        public void ShowCredentialsLoading(CredentialsLoadNotificationViewModel viewModel)
        {

            if (UIDispatcher.CheckAccess())
            {
                notifier.ShowCredentialsLoading(viewModel);
            }
            else
            {
                // Do non UI Thread stuff
                UIDispatcher.Invoke(() => notifier.ShowCredentialsLoading(viewModel));
            }
        }

        public void CloseWindow(Guid id)
        {
            UIDispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext is IRequireViewIdentification vm && vm.ID.Equals(id))
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

        #region PIN

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

        public void ShowPinNotVerified(Device device)
        {
            if (UIDispatcher.CheckAccess())
            {
                notifier.ShowPinNotVerified(device);
            }
            else
            {
                UIDispatcher.Invoke(() => notifier.ShowPinNotVerified(device));
            }
        }

        public void ShowSetPin(Device device)
        {
            SetPinViewModel viewModel = viewModelLocator.SetPinViewModel;
            viewModel.Device = device;
            viewModel.State = ViewPinState.WaitUserAction;
            viewModel.ButtonState = ConfirmButtonState.None;
            ShowPinViewAsync(viewModel);
        }

        public void ShowChangePin(Device device)
        {
            ChangePinViewModel viewModel = viewModelLocator.ChangePinViewModel;
            viewModel.Device = device;
            viewModel.State = ViewPinState.WaitUserAction;
            viewModel.ButtonState = ConfirmButtonState.None;
            ShowPinViewAsync(viewModel);
        }

        public Task<bool> ShowDialogEnterPinAsync(EnterPinViewModel viewModel)
        {
            return ShowPinViewAsync(viewModel);
        }

        private Task<bool> ShowPinViewAsync(PinViewModelBase viewModel)
        {
            return UIDispatcher.InvokeAsync(() =>
            {
                bool mainWindowWasOpen = IsMainWindowVisible;
                PinView view = new PinView();
                view.DataContext = viewModel;
                SetStartupLocation(view, mainWindowWasOpen, true);
                bool dialogResult = view.ShowDialog() ?? false;
                return dialogResult;
            }).Task;
        }

        #endregion PIN


        public void ShowInfoAboutDevice(Device device)
        {
            UIDispatcher.Invoke(() =>
            {
                AboutDeviceView view = new AboutDeviceView();
                view.Owner = MainWindow;
                view.DataContext = new DeviceViewModel(device);
                view.Show();
            });
        }
    }
}

