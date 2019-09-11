using GalaSoft.MvvmLight.Messaging;
using HideezClient.Messages;
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

        public Uri DisplayPage
        {
            get { return displayPage; }
            set { Set(ref displayPage, value); }
        }

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        #endregion Properties

        public void ProcessNavRequest(string page)
        {
            DisplayPage = new Uri($"pack://application:,,,/HideezClient;component/PagesView/{page}.xaml", UriKind.Absolute);
        }

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
                            OnShowLocker();
                    },
                };
            }
        }

        #endregion Command

        private void OnShowLocker()
        {
            ProcessNavRequest("LoginSystemPage");
        }
    }
}
