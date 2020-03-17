using HideezClient.Controls;
using HideezClient.Models;
using HideezClient.Modules.Localize;
using HideezClient.Mvvm;
using HideezClient.Utilities;
using HideezClient.ViewModels;
using HideezClient.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HideezClient.Modules
{
    // Todo: Create base class for all notification types and refactor logic for notification adding, removal and timeout so tha its tied only to Content & NotificationId
    // Actually, the content may only be relevant to the generic notifications and the rest might work exclusivelly with NotificationId
    class Notifier : INotifier, IDisposable
    {
        readonly object lockObj = new object();

        Dictionary<string, NotificationsContainerWindow> windowsForNotifications = new Dictionary<string, NotificationsContainerWindow>();

        static HashSet<string> viewLoadingCredentialsForDevices = new HashSet<string>();
        static Dictionary<string, NotificationBase> displayedNotAuthorizedDeviceNotifications = new Dictionary<string, NotificationBase>();
        static Dictionary<string, NotificationBase> displayedDeviceIsLockedNotifications = new Dictionary<string, NotificationBase>();

        public Notifier()
        {
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            ClearContainers();
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed = false;
        void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {

            }

            // Because this is a static event, you must detach your event handlers when your application is disposed.
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;

            disposed = true;
        }

        ~Notifier()
        {
            Dispose(false);
        }
        #endregion
        
        public void ShowInfo(string title, string message, NotificationOptions options = null)
        {
            ShowInfo(string.Empty, title, message, options);
        }

        public void ShowInfo(string notificationId, string title, string message, NotificationOptions options = null)
        {
            ShowSimpleNotification(notificationId, title, message, options, NotificationIconType.Info);
        }


        public void ShowWarn(string title, string message, NotificationOptions options = null)
        {
            ShowWarn(string.Empty, title, message, options);
        }

        public void ShowWarn(string notificationid, string title, string message, NotificationOptions options = null)
        {
            ShowSimpleNotification(notificationid, title, message, options, NotificationIconType.Warn);
        }


        public void ShowError(string title, string message, NotificationOptions options = null)
        {
            ShowError(string.Empty, title, message, options);
        }

        public void ShowError(string notificationId, string title, string message, NotificationOptions options = null)
        {
            ShowSimpleNotification(notificationId, title, message, options, NotificationIconType.Error);
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

        public void ShowStorageLoadingNotification(CredentialsLoadNotificationViewModel viewModel)
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
                notification = new CredentialsLoadNotification(options)
                {
                    DataContext = viewModel
                };
                AddNotification(screen, notification);

                notification.Closed += (sender, e) => viewLoadingCredentialsForDevices.Remove(viewModel.DeviceSN);
            }
        }

        public void ShowDeviceNotAuthorizedNotification(Device device)
        {
            // Prevent multiple not authorized notifications for the same device
            if (!displayedNotAuthorizedDeviceNotifications.Keys.Contains(device.SerialNo))
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Screen screen = GetCurrentScreen();
                    DeviceNotAuthorizedNotification notification = new DeviceNotAuthorizedNotification(new NotificationOptions { SetFocus = true });
                    if (notification.DataContext is DeviceNotAuthorizedNotificationViewModel viewModel)
                    {
                        viewModel.Device = device;
                        notification.Closed += (sender, e) => displayedNotAuthorizedDeviceNotifications.Remove(device.SerialNo);
                        AddNotification(screen, notification);
                        displayedNotAuthorizedDeviceNotifications.Add(device.SerialNo, notification);
                    }
                });
            }
            else
            {
                displayedNotAuthorizedDeviceNotifications[device.SerialNo]?.ResetCloseTimer();
            }
        }

        public void ShowDeviceIsLockedNotification(Device device)
        {
            var options = new NotificationOptions()
            {
                CloseTimeout = TimeSpan.FromSeconds(20)
            };

            ShowSimpleNotification(device.SerialNo + "_Locked",
                TranslationSource.Instance["Notification.DeviceLocked.Caption"],
                TranslationSource.Instance["Notification.DeviceLocked.Message"],
                options,
                NotificationIconType.Lock);
            /*
            // Prevent multiple not authorized notifications for the same device
            if (!displayedDeviceIsLockedNotifications.Keys.Contains(device.SerialNo))
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    Screen screen = GetCurrentScreen();
                    var options = new NotificationOptions 
                    { 
                        CloseTimeout = TimeSpan.FromSeconds(20) 
                    };
                    SimpleNotification notification = new SimpleNotification(options, NotificationIconType.Lock);
                    if (notification.DataContext is DeviceIsLockedNotificationViewModel viewModel)
                    {
                        viewModel.Device = device;
                        notification.Closed += (sender, e) => displayedNotAuthorizedDeviceNotifications.Remove(device.SerialNo);
                        AddNotification(screen, notification);
                        displayedDeviceIsLockedNotifications.Add(device.SerialNo, notification);
                    }
                });
            }
            else
            {
                displayedDeviceIsLockedNotifications[device.SerialNo]?.ResetCloseTimer();
            }
            */
        }

        void ShowSimpleNotification(string notificationId, string title, string message, NotificationOptions options, NotificationIconType notificationType)
        {
            Screen screen = GetCurrentScreen();

            if (string.IsNullOrWhiteSpace(notificationId))
                notificationId = Guid.NewGuid().ToString();

            // If notification with matching ID and content is found, extend its duration
            // If ID matches but content is different, close old notification and display a new one
            // If no copy is found, show new notification
            var matchingNotificationView = GetNotifications().FirstOrDefault(n => (n.DataContext as SimpleNotificationViewModel)?.ObservableId == notificationId);
            if (matchingNotificationView?.DataContext is SimpleNotificationViewModel matchingNotificationViewModel)
            {
                if (matchingNotificationViewModel.Message == message &&
                matchingNotificationViewModel.Title == title)
                {
                    matchingNotificationView.ResetCloseTimer();
                    return;
                }
                else
                {
                    matchingNotificationView.Close();
                }
            }

            // Do not create notifications without content
            if (string.IsNullOrWhiteSpace(message))
                return;

            SimpleNotification notification = new SimpleNotification(options ?? new NotificationOptions(), notificationType)
            {
                DataContext = new SimpleNotificationViewModel { Title = title, Message = message, ObservableId = notificationId, }
            };
            AddNotification(screen, notification);
        }

        /// <summary>
        /// Find container for notification by screen if not found container create new and then add notification to container associated with the screen
        /// </summary>
        /// <param name="screen">Screen where show notification</param>
        /// <param name="notification">Notification</param>
        /// <param name="addForce">If tru Add to stack notifications if count notification more then max</param>
        void AddNotification(Screen screen, NotificationBase notification, bool addForce = false)
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
            window.Show();
        }

        Screen GetCurrentScreen()
        {
            IntPtr foregroundWindow = Win32Helper.GetForegroundWindow();
            Screen screen = Screen.FromHandle(foregroundWindow);
            return screen;
        }

        IEnumerable<NotificationBase> GetNotifications()
        {
            return windowsForNotifications
                .Values
                .OfType<NotificationsContainerWindow>()
                .SelectMany(vm => (vm.DataContext as NotificationsContainerViewModel).Items)
                .ToList();
        }

        /// <summary>
        /// Close all windows for notification if screens is not valid
        /// Example: disconnect one or more monitors
        /// </summary>
        void ClearContainers()
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
    }
}
