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
            Proximity = device.Proximity;
            IsConnected = device.IsConnected;
            Battery = device.Battery;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Owner { get; set; }

        [DataMember]
        public double Proximity { get; set; }

        [DataMember]
        public bool IsConnected { get; set; }

        [DataMember]
        public int Battery { get; set; }

        [DataMember]
        public string SerialNo { get; set; }

        [DataMember]
        public Version FirmwareVersion { get; private set; }

        [DataMember]
        public Version BootloaderVersion { get; private set; }

        [DataMember]
        public uint StorageTotalSize { get; private set; }

        [DataMember]
        public uint StorageFreeSize { get; private set; }

        [DataMember]
        public bool IsInitialized { get; private set; }
    }
}
