using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HideezSafe.Modules
{
    class BalloonTipNotifyManager : IBalloonTipNotifyManager
    {
        private readonly TaskbarIcon taskbarIcon;

        public BalloonTipNotifyManager(TaskbarIcon taskbarIcon)
        {
            this.taskbarIcon = taskbarIcon;

            this.taskbarIcon.TrayBalloonTipClicked += OnClicked;
            this.taskbarIcon.TrayBalloonTipShown += OnShown;
            this.taskbarIcon.TrayBalloonTipClosed += OnClosed;
        }

        public void ShowError(string title, string description)
        {
            taskbarIcon.ShowBalloonTip(title, description, BalloonIcon.Error);
        }

        public void ShowWarning(string title, string description)
        {
            taskbarIcon.ShowBalloonTip(title, description, BalloonIcon.Warning);
        }

        public void ShowInfo(string title, string description)
        {
            taskbarIcon.ShowBalloonTip(title, description, BalloonIcon.Info);
        }

        private void OnClosed(object sender, RoutedEventArgs e)
        {
        }

        private void OnShown(object sender, RoutedEventArgs e)
        {
        }

        private void OnClicked(object sender, RoutedEventArgs e)
        {
        }
    }
}
