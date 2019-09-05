using Hideez.SDK.Communication.Interfaces;
using System;
using System.Runtime.Serialization;

namespace ServiceLibrary
{
    [DataContract]
    public class DeviceDTO
    {
        public DeviceDTO(IDevice device)
        {
            Id = device.Id;
            Name = device.Name;
            IsConnected = device.IsConnected;
            IsBoot = device.IsBoot;
            Battery = device.Battery;
            SerialNo = device.SerialNo;
            FirmwareVersion = device.FirmwareVersion;
            BootloaderVersion = device.BootloaderVersion;
            IsInitialized = device.IsInitialized;
            StorageTotalSize = device.StorageTotalSize;
            StorageFreeSize = device.StorageFreeSize;
            IsAuthorized = device.IsAuthorized;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string SerialNo { get; set; }

        [DataMember]
        public string Owner { get; set; }

        [DataMember]
        public bool IsConnected { get; set; }

        [DataMember]
        public bool IsBoot { get; private set; }

        [DataMember]
        public sbyte Battery { get; set; }

        [DataMember]
        public Version FirmwareVersion { get; private set; }

        [DataMember]
        public Version BootloaderVersion { get; private set; }

        [DataMember]
        public bool IsInitialized { get; private set; }

        [DataMember]
        public bool IsAuthorized { get; private set; }

        [DataMember]
        public uint StorageTotalSize { get; private set; }

        [DataMember]
        public uint StorageFreeSize { get; private set; }
    }
}
