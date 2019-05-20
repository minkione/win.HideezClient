using HideezSafe.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules.DeviceManager
{
    interface IDeviceManager
    {
        ObservableCollection<DeviceViewModel> Devices { get; }
    }
}
