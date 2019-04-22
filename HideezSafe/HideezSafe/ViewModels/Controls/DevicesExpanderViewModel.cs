using HideezSafe.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.ViewModels
{
    class DevicesExpanderViewModel : ObservableObject
    {
        public DevicesExpanderViewModel()
        {
#if DEBUG
            currentDevice = new DeviceViewModel("DeviceType.Key", "HedeezKeySimpleIMG", "8989")
            {
                IsConnected = false,
                Proximity = 60,
            };

            OwnerName = "User@mail.com";
#endif
        }

        #region Properties

        private DeviceViewModel currentDevice;
        private string ownerName;

        public DeviceViewModel CurrentDevice
        {
            get { return currentDevice; }
            set { Set(ref currentDevice, value); }
        }


        public string OwnerName
        {
            get { return ownerName; }
            set { Set(ref ownerName, value); }
        }

        #endregion Properties
    }
}
