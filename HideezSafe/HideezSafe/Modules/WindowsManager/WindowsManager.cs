using MvvmExtentions.EventAggregator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace HideezSafe.Modules
{
    class WindowsManager : IWindowsManager
    {
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

        private Window MainWindow { get { return Application.Current.MainWindow; } }

        private Dispatcher UIDispatcher { get { return Application.Current.Dispatcher; } }

        private void OnActivateMainWindow()
        {
            if (MainWindow == null) return;

            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.WindowState = WindowState.Normal;
            }

            MainWindow.Show();
            MainWindow.Activate();
        }
    }
}
