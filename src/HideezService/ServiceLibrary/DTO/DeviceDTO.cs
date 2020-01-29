using Hideez.SDK.Communication.Interfaces;
using HideezMiddleware;
using System;
using System.Runtime.Serialization;

namespace ServiceLibrary
{
    [DataContract]
    public class DeviceDTO
    {
        public DeviceDTO(string id, string name)
        {
            Id = id;
            Name = name;
            IsConnected = true;
            IsBoot = false;
            Battery = 100;
            SerialNo = "ST103111320343";
            FirmwareVersion = new Version(3, 0, 0);
            BootloaderVersion = new Version(2, 0, 0);
            IsInitialized = true;
            StorageTotalSize = 70;
            StorageFreeSize = 70;
            IsAuthorized = true;
            PinAttemptsRemain = 2;
            FinishedMainFlow = true;
            Proximity = 80;
            CanLockPyProximity = false;
        }

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
            PinAttemptsRemain = device.PinAttemptsRemain;
            FinishedMainFlow = device.GetUserProperty<bool>(ConnectionFlowProcessor.FLOW_FINISHED_PROP);
            Proximity = device.Proximity;
            CanLockPyProximity = device.GetUserProperty<bool>(WorkstationLockProcessor.PROX_LOCK_ENABLED_PROP);
            OwnerName = device.GetUserProperty<string>(ConnectionFlowProcessor.OWNER_NAME_PROP) ?? string.Empty;
            OwnerEmail = device.GetUserProperty<string>(ConnectionFlowProcessor.OWNER_EMAIL_PROP)?? string.Empty;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string SerialNo { get; set; }

        [DataMember]
        public string OwnerName { get; set; }

        [DataMember]
        public string OwnerEmail { get; set; }

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

        [DataMember]
        public int PinAttemptsRemain { get; private set; }

        [DataMember]
        public bool FinishedMainFlow { get; private set; }

        [DataMember]
        public double Proximity { get; private set; }
        
        [DataMember]
        public bool CanLockPyProximity { get; private set; }
    }
}
