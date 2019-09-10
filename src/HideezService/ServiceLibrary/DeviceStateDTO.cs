using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Interfaces;
using System.Runtime.Serialization;

namespace ServiceLibrary
{
    [DataContract]
    public class DeviceStateDTO
    {
        public DeviceStateDTO(DeviceState state)
        {
            Battery = state.Battery;
            Rssi = state.Rssi;
            PinAttemptsRemain = state.PinAttemptsRemain;
            StorageUpdateCounter = state.StorageUpdateCounter;
            Button = state.Button;
            AccessLevel = new AccessLevelDTO(state.AccessLevel);
        }

        [DataMember]
        public sbyte Battery { get; set; }

        [DataMember]
        public sbyte Rssi { get; set; }

        [DataMember]
        public byte PinAttemptsRemain { get; set; }

        [DataMember]
        public byte StorageUpdateCounter { get; set; }

        [DataMember]
        public ButtonPressCode Button { get; set; }

        [DataMember]
        public AccessLevelDTO AccessLevel { get; set; }
    }
}
