using HideezSafe.HideezServiceReference;
using HideezSafe.Modules.Localize;
using HideezSafe.Mvvm;
using MvvmExtensions.Attributes;

namespace HideezSafe.ViewModels
{
    public class DeviceViewModel : LocalizedObject
    {
        public DeviceViewModel(string owner, string typeName, string icoKey, string name)
        {
            this.ownerName = owner;
            this.typeNameKey = typeName;
            this.IcoKey = icoKey;
            this.name = name;
        }

        public DeviceViewModel(BleDeviceDTO device)
        {
            LoadFrom(device);
        }

        #region Property

        private string id;
        private bool isConnected;
        private int proximity;

        public string IcoKey { get; }

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

        public int Proximity
        {
            get { return proximity; }
            set { Set(ref proximity, value); }
        }

        #region Text

        private string name;
        private string typeNameKey;
        private string ownerName = "... unspecified ...";

        public string OwnerName
        {
            get { return ownerName; }
            set { Set(ref ownerName, value); }
        }

        [Localization]
        [DependsOn(nameof(TypeName))]
        public string Name
        {
            get { return $"{L(typeNameKey)} - {name}"; }
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
        }
    }
}
