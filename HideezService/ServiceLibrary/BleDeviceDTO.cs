using Hideez.SDK.Communication.BLE;
using System.Runtime.Serialization;

namespace ServiceLibrary
{
    [DataContract]
    public class BleDeviceDTO
    {
        public BleDeviceDTO(BleDevice device)
        {

        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}
