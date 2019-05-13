using HideezSafe.Modules.DeviceManager;
using HideezSafe.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace HideezSafe.ViewModels
{
    class DevicesExpanderViewModel : ObservableObject
    {
        readonly IDeviceManager deviceManager;

        public DevicesExpanderViewModel(IDeviceManager deviceManager)
        {
            this.deviceManager = deviceManager;
        }

        #region Properties


        public ObservableCollection<DeviceViewModel> Devices
        {
            get
            {
                return deviceManager.Devices;
            }
        }

        #endregion Properties
    }
}
