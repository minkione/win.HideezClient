using Hideez.SDK.Communication.Interfaces;
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
            DeviceInfo = device.DeviceInfo;
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
        public IDeviceInfo DeviceInfo { get; set; }
    }
}
