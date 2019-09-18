using HideezClient.Mvvm;
using HideezClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.PageViewModels
{
    class AboutDevicePageViewModel : LocalizedObject
    {
        private DeviceViewModel device;
        public DeviceViewModel Device
        {
            get { return device; }
            set { Set(ref device, value); }
        }
    }
}
