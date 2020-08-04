using Hideez.SDK.Communication.Device;
using System;
using System.Runtime.Serialization;

namespace HideezMiddleware.IPC.DTO
{
    [DataContract]
    public class AccessLevelDTO
    {
        public AccessLevelDTO()
        {
        }

        public AccessLevelDTO(AccessLevel accessLevel)
        {
            IsLinkRequired = accessLevel.IsLinkRequired;
            IsNewPinRequired = accessLevel.IsNewPinRequired;
            IsMasterKeyRequired = accessLevel.IsMasterKeyRequired;
            IsPinRequired = accessLevel.IsPinRequired;
            IsButtonRequired = accessLevel.IsButtonRequired;
            IsLocked = accessLevel.IsLocked;
        }

        [DataMember]
        public bool IsLinkRequired { get; set; }

        [DataMember]
        public bool IsNewPinRequired { get; set; }

        [DataMember]
        public bool IsMasterKeyRequired { get; set; }

        [DataMember]
        public bool IsPinRequired { get; set; }

        [DataMember]
        public bool IsButtonRequired { get; set; }

        [DataMember]
        public bool IsLocked { get; set; }
    }
}
