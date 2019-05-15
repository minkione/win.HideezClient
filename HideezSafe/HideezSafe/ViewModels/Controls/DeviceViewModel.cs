using HideezSafe.HideezServiceReference;
using HideezSafe.Modules.Localize;
using HideezSafe.Mvvm;
using MvvmExtensions.Attributes;

namespace HideezSafe.ViewModels
{
    public class DeviceViewModel : LocalizedObject
    {
        public DeviceViewModel(BleDeviceDTO device)
        {
            LoadFrom(device);
        }

        #region Property

        private string id;
        private bool isConnected;
        private double proximity;

        public string IcoKey { get; } = "HedeezKeySimpleIMG";

        public string Id
        {
            get { return id; }
            set { Set(ref id, value); }
        }

        public bool IsConnected
        {
            get { return isConnected; }
            set { Set(ref isConnected, value); }
        }

        public double Proximity
        {
            get { return proximity; }
            set { Set(ref proximity, value); }
        }

        #region Text

        private string name;
        private string typeNameKey = "Hideez key";
        private string ownerName;

        public string OwnerName
        {
            get { return ownerName; }
            set { Set(ref ownerName, value); }
        }

        [Localization]
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        [Localization]
        public string TypeName
        {
            get { return L(typeNameKey); }
            set { Set(ref typeNameKey, value); }
        }

        #endregion Text

        #endregion Property

        public void LoadFrom(BleDeviceDTO dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Proximity = dto.Proximity;
            OwnerName = dto.Owner ?? "...unspecified...";
            this.IsConnected = dto.IsConnected;
        }
    }
}
