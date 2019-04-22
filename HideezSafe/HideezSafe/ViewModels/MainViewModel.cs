using GalaSoft.MvvmLight.Command;
using HideezSafe.Controls;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Mvvm;
using MvvmExtensions.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace HideezSafe.ViewModels
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

        #endregion Properties

        public void ProcessNavRequest(string page)
        {
            DisplayPage = new Uri($"pack://application:,,,/HideezSafe;component/PagesView/{page}.xaml", UriKind.Absolute);
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
