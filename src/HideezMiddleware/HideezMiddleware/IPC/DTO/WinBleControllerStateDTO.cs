using System.Runtime.Serialization;

namespace HideezMiddleware.IPC.DTO
{
    [DataContract]
    public class WinBleControllerStateDTO
    {
        public WinBleControllerStateDTO()
        { 
        }

        public WinBleControllerStateDTO(WinBleControllerState controllerState)
        {
            Id = controllerState.Id;
            Name = controllerState.Name;
            Mac = controllerState.Mac;
            IsConnected = controllerState.IsConnected;
            IsDiscovered = controllerState.IsDiscovered;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Mac { get; set; }

        [DataMember]
        public bool IsConnected { get; set; }

        [DataMember]
        public bool IsDiscovered { get; set; }
    }
}
