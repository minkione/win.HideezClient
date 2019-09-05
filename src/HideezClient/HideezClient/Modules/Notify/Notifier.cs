using HideezClient.Controls;
using HideezClient.Models;
using HideezClient.Modules.ActionHandler;
using HideezClient.Mvvm;
using HideezClient.Utilities;
using HideezClient.ViewModels;
using HideezClient.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace HideezClient.Modules
{
    class Notifier : INotifier, IDisposable
    {
        private Dictionary<string, NotificationsContainerWindow> windowsForNotifications = new Dictionary<string, NotificationsContainerWindow>();
        private readonly object lockObj = new object();

        public Notifier()
        {
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            ClearContainers();
        }

        public void ShowInfo(string title, string message, NotificationOptions options = null)
        {
            ShowSimpleNotification(title, message, options, SimpleNotificationType.Info);
        }

        public void ShowWarn(string title, string message, NotificationOptions options = null)
        {
            ShowSimpleNotification(title, message, options, SimpleNotificationType.Warn);
        }

        public void ShowError(string title, string message, NotificationOptions options = null)
        {
            ShowSimpleNotification(title, message, options, SimpleNotificationType.Error);
        }

        private void ShowSimpleNotification(string title, string message, NotificationOptions options, SimpleNotificationType notificationType)
        {
            Screen screen = GetCurrentScreen();
            SimpleNotification notification = new SimpleNotification(options ?? new NotificationOptions(), notificationType)
            {
                DataContext = new SimpleNotificationViewModel { Title = title, Message = message, }
            };
            AddNotification(screen, notification);
        }

        /// <summary>
        /// Close all windows for notification if screens is not valid
        /// Example: disconnect one or more monitors
        /// </summary>
        private void ClearContainers()
        {
            try
            {
                lock (lockObj)
                {
                    foreach (var screen in windowsForNotifications.Keys.Except(Screen.AllScreens.Select(s => s.DeviceName)).ToArray())
                    {
                        if (windowsForNotifications.TryGetValue(screen, out NotificationsContainerWindow window))
                        {
                            window.Close();
                            windowsForNotifications.Remove(screen);
                        }
                    }
                }
            }
            catch { }
        }

        public async Task<Account> SelectAccountAsync(Account[] accounts, IntPtr hwnd)
        {
            ClearContainers();

            TaskCompletionSource<bool> taskCompletionSourceForDialog = new TaskCompletionSource<bool>();

            var viewModel = new AccountSelectorViewModel(accounts);
            AccountSelector notification = new AccountSelector(new NotificationOptions { SetFocus = true, CloseWhenDeactivate = true, Position = NotificationPosition.Bottom, TaskCompletionSource = taskCompletionSourceForDialog, })
            {
                DataContext = viewModel,
            };

            Screen screen = Screen.FromHandle(hwnd);
            AddNotification(screen, notification, true);
            bool dialogResalt = await taskCompletionSourceForDialog.Task;
            if (dialogResalt)
            {
                return viewModel.SelectedAccount.Account;
            }

            return null;
        }

        /// <summary>
        /// Find container for notification by screen if not found container create new and then add notification to container associated with the screen
        /// </summary>
        /// <param name="screen">Screen where show notification</param>
        /// <param name="notification">Notification</param>
        /// <param name="addForce">If tru Add to stack notifications if count notification more then max</param>
        private void AddNotification(Screen screen, NotificationBase notification, bool addForce = false)
        {
            NotificationsContainerWindow window = null;
            lock (lockObj)
            {
                windowsForNotifications.TryGetValue(screen.DeviceName, out window);
                if (window == null)
                {
                    window = new NotificationsContainerWindow(screen);
                    window.Show();

                    windowsForNotifications[screen.DeviceName] = window;
                }
            }

           (window.DataContext as NotificationsContainerViewModel)?.AddNotification(notification, addForce);
        }

        public void Dispose()
        {
            // Because this is a static event, you must detach your event handlers when your application is disposed.
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
        }

        private static HashSet<string> viewLoadingCredentialsForDevices = new HashSet<string>();
        public void ShowCredentialsLoading(CredentialsLoadNotificationViewModel viewModel)
        {
            if (!viewLoadingCredentialsForDevices.Contains(viewModel.DeviceSN))
            {
                viewLoadingCredentialsForDevices.Add(viewModel.DeviceSN);
                Screen screen = GetCurrentScreen();
                NotificationOptions options = new NotificationOptions
                {
                    CloseTimeout = TimeSpan.Zero,
                };

                CredentialsLoadNotification notification = null;
                notification = new CredentialsLoadNotification(options);
                notification.DataContext = viewModel;
                AddNotification(screen, notification);

                notification.Closed += (sender, e) => viewLoadingCredentialsForDevices.Remove(viewModel.DeviceSN);
            }
        }

        // Todo: Refactor this from PIN to not authorized device
        private static Dictionary<string, Guid> pinNotVerifiedForDevices = new Dictionary<string, Guid>();
        public void ShowDeviceNotAuthorized(Device device)
        {
            if (!pinNotVerifiedForDevices.Keys.Contains(device.SerialNo))
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Screen screen = GetCurrentScreen();
                    PinNotVerifiedNotification notification = new PinNotVerifiedNotification(new NotificationOptions { SetFocus = true });
                    if (notification.DataContext is PinNotVerifiedNotificationViewModel viewModel)
                    {
                        viewModel.Device = device;
                        notification.Closed += (sender, e) => pinNotVerifiedForDevices.Remove(device.SerialNo);
                        AddNotification(screen, notification);
                        pinNotVerifiedForDevices.Add(device.SerialNo, viewModel.ID);
                    }
                });
            }
            else
            {
                GetNotifications(pinNotVerifiedForDevices[device.SerialNo]).ToList().ForEach(n => n.ResetCloseTimer());
            }
        }

        private Screen GetCurrentScreen()
        {
            IntPtr foregroundWindow = Win32Helper.GetForegroundWindow();
            Screen screen = Screen.FromHandle(foregroundWindow);
            return screen;
        }

        public IEnumerable<NotificationBase> GetNotifications(Guid id)
        {
            return windowsForNotifications.Values.OfType<NotificationsContainerViewModel>()
                .SelectMany(vm => vm.Items.Where(nb => (nb.DataContext as IRequireViewIdentification)?.ID == id));
        }

        public void CloseNotifications(Guid id)
        {
            GetNotifications(id).ToList().ForEach(n => n.Close());
        }
    }
}
