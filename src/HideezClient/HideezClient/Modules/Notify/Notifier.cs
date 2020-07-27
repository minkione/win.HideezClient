using HideezClient.Controls;
using HideezClient.Models;
using HideezClient.Modules.Localize;
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

        public void ClearNotifications()
        {
            var notifications = GetNotifications();
            foreach (NotificationBase notification in notifications)
            {
                notification.Close();
            }
        }


        public async Task<Account> SelectAccountAsync(Account[] accounts, IntPtr hwnd)
        {
            ClearContainers();

            TaskCompletionSource<bool> taskCompletionSourceForDialog = new TaskCompletionSource<bool>();

            var options = new NotificationOptions
            {
                SetFocus = true,
                CloseWhenDeactivate = true,
                Position = NotificationPosition.Bottom,
                TaskCompletionSource = taskCompletionSourceForDialog,
                CloseTimeout = NotificationOptions.LongTimeout,
            };

            var viewModel = new AccountSelectorViewModel(accounts);
            AccountSelector notification = new AccountSelector(options)
            {
                DataContext = viewModel,
            };

            Screen screen = Screen.FromHandle(hwnd);
            AddNotification(screen, notification, true);
            bool dialogResult = await taskCompletionSourceForDialog.Task;
            if (dialogResult)
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
                var options = new NotificationOptions { CloseTimeout = TimeSpan.Zero, };

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

        public void ShowDeviceIsLockedByPinNotification(Device device)
        {
            var options = new NotificationOptions() { CloseTimeout = NotificationOptions.LongTimeout };

            ShowSimpleNotification(device.SerialNo + "_Locked",
                TranslationSource.Instance["Notification.DeviceLockedByPin.Caption"],
                TranslationSource.Instance["Notification.DeviceLockedByPin.Message"],
                options,
                NotificationIconType.Lock);
        }

        public void ShowDeviceIsLockedByCodeNotification(Device device)
        {
            var options = new NotificationOptions() { CloseTimeout = NotificationOptions.LongTimeout };

            ShowSimpleNotification(device.SerialNo + "_Locked",
                TranslationSource.Instance["Notification.DeviceLockedByCode.Caption"],
                TranslationSource.Instance["Notification.DeviceLockedByCode.Message"],
                options,
                NotificationIconType.Lock);
        }

        public async Task<bool> ShowAccountNotFoundNotification(string title, string message)
        {
            ClearContainers();

            TaskCompletionSource<bool> taskCompletionSourceForDialog = new TaskCompletionSource<bool>();

            var options = new NotificationOptions()
            {
                CloseTimeout = NotificationOptions.LongTimeout,
                SetFocus = true,
                CloseWhenDeactivate = true,
                TaskCompletionSource = taskCompletionSourceForDialog,
            };

            var viewModel = new SimpleNotificationViewModel()
            {
                Title = title,
                //Message = message + Environment.NewLine + "Create new account?",
                Message = message
            };
            AccountNotFoundNotification notification = new AccountNotFoundNotification(options)
            {
                DataContext = viewModel,
            };

            Screen screen = GetCurrentScreen();
            AddNotification(screen, notification, true);
            var result = await taskCompletionSourceForDialog.Task;
            return result;
        }

        void ShowSimpleNotification(string notificationId, string title, string message, NotificationOptions options, NotificationIconType notificationType)
        {
            Screen screen = GetCurrentScreen();

            if (string.IsNullOrWhiteSpace(notificationId))
                notificationId = Guid.NewGuid().ToString();

            // Check if there are any notifications with same id
            bool foundMatchingContent = false;
            var matchingNotificationViews = GetNotifications().Where(n => (n.DataContext as SimpleNotificationViewModel)?.ObservableId == notificationId);
            foreach (var notificationView in matchingNotificationViews)
            {
                if (notificationView.DataContext is SimpleNotificationViewModel matchingNotificationViewModel)
                {
                    // If notification with matching ID and content is found, extend its duration
                    // If ID matches but content is different, close old notification and display a new one
                    // TODO: Change notification content comparison from using string to using hash
                    if (matchingNotificationViewModel.Message == message &&
                    matchingNotificationViewModel.Title == title)
                    {
                        notificationView.ResetCloseTimer();
                        foundMatchingContent = true;
                    }
                    else
                    {
                        notificationView.Close();
                    }
                }
            }

            // No need to create duplicate notifications
            if (foundMatchingContent)
                return;

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
