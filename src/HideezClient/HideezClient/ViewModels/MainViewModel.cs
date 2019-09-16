using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
using HideezClient.Models;
using HideezClient.Mvvm;
using MvvmExtensions.Commands;
using System;
using System.Reflection;
using System.Windows.Input;

namespace HideezClient.ViewModels
{
    class MainViewModel : ObservableObject
    {
        #region Properties

        private Uri displayPage;
        private Device device;

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Uri DisplayPage
        {
            get { return displayPage; }
            set { Set(ref displayPage, value); }
        }

        public Device SelectedDevice
        {
            get { return device; }
            set { Set(ref device, value); }
        }

        #endregion Properties

        #region Command

        public ICommand ShowLockerCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        if (x is bool isChecked && isChecked)
                            OnOpenLocker();
                    },
                };
            }
        }

        public ICommand OpenHelpCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x => OnOpenHelpCommand(),
                };
            }
        }

        public ICommand OpenSettingsCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x => OnOpenSettingsCommand(),
                };
            }
        }

        #endregion Command


        #region Navigation

        public void ProcessNavRequest(string page)
        {
            DisplayPage = new Uri($"pack://application:,,,/HideezClient;component/PagesView/{page}.xaml", UriKind.Absolute);
        }

        private void OnOpenLocker()
        {
            ProcessNavRequest("LoginSystemPage");
        }

        private void OnOpenSettingsCommand()
        {
            ProcessNavRequest("SettingsPage");
        }

        private void OnOpenHelpCommand()
        {
            ProcessNavRequest("HelpPage");
        }

        #endregion
    }
}
