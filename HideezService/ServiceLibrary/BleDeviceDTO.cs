using Hideez.SDK.Communication.BLE;
using System.Runtime.Serialization;

namespace ServiceLibrary
{
    [DataContract]
    public class BleDeviceDTO
    {
        public BleDeviceDTO(BleDevice device)
        {
            this.Id = device.Id;
            this.Name = device.Name;
            this.Proximity = device.Proximity;
            this.IsConnected = device.IsConnected;
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
    }
}
