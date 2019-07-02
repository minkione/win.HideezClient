using HideezSafe.Models;
using HideezSafe.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HideezSafe.Modules.DeviceManager
{
    interface IDeviceManager
    {
        ObservableCollection<Device> Devices { get; }
    }
}
