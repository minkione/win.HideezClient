using HideezSafe.Modules.DeviceManager;
using HideezSafe.Mvvm;
using System.Linq;

namespace HideezSafe.ViewModels
{
    class DevicesExpanderViewModel : ObservableObject
    {
        readonly IDeviceManager deviceManager;
        string ownerName;

        public DevicesExpanderViewModel(IDeviceManager deviceManager)
        {
            this.deviceManager = deviceManager;
            OwnerName = "... unspecified ...";
            deviceManager.Devices.CollectionChanged += DeviceManagerDevices_CollectionChanged;
        }

        #region Properties


        public DeviceViewModel CurrentDevice
        {
            get
            {
                return deviceManager.Devices.FirstOrDefault();
            }
        }

        public string OwnerName
        {
            get { return ownerName; }
            set { Set(ref ownerName, value); }
        }

        #endregion Properties

        private void DeviceManagerDevices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(CurrentDevice));
        }
    }
}
