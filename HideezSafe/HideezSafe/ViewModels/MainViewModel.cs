using GalaSoft.MvvmLight.Command;
using HideezSafe.Controls;
using HideezSafe.Modules;
using HideezSafe.Modules.Localize;
using HideezSafe.Mvvm;
using MvvmExtentions.Commands;
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
        public MainViewModel()
        {
#if DEBUG
            StateControls.Add(new StatusControlViewModel { Status = false, Header = "Status.HES", FalseToolTip = "Status.Tooltip.DisconectedHES", TrueToolTip = "Status.Tooltip.ConectedHES" });
            StateControls.Add(new StatusControlViewModel { Status = true, Header = "Status.RFID", FalseToolTip = "Status.Tooltip.DisconectedRFID", TrueToolTip = "Status.Tooltip.ConectedRFID" });
            StateControls.Add(new StatusControlViewModel { Status = true, Header = "Status.Dongle", FalseToolTip = "Status.Tooltip.DisconectedDongle", TrueToolTip = "Status.Tooltip.ConectedDongle" });

            currentDevice = new DeviceViewModel("DeviceType.Key", "HedeezKeySimpleIMG", "8989");
#endif
        }

        #region Properties

        private Uri displayPage;
        private DeviceViewModel currentDevice;

        public IList<StatusControlViewModel> StateControls { get; } = new List<StatusControlViewModel>();

        public Uri DisplayPage
        {
            get { return displayPage; }
            set { Set(ref displayPage, value); }
        }

        public DeviceViewModel CurrentDevice
        {
            get { return currentDevice; }
            set { Set(ref currentDevice, value); }
        }

        #endregion Properties

        public void ProcessNavRequest(string page)
        {
            DisplayPage = new Uri($"pack://application:,,,/HideezSafe;component/PagesView/{page}.xaml", UriKind.Absolute);
        }

        #region Command

        public ICommand ShowLokerCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        if (x is bool isChecked && isChecked)
                            OnShowLoker();
                    },
                };
            }
        }

        #endregion Command

        private void OnShowLoker()
        {
            ProcessNavRequest("LoginSystemPage");
        }
    }
}
