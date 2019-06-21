using HideezSafe.ViewModels;
using System.Collections.ObjectModel;

namespace HideezSafe.Modules.DeviceManager
{
    interface IDeviceManager
    {
        ObservableCollection<DeviceViewModel> Devices { get; }
    }
}
