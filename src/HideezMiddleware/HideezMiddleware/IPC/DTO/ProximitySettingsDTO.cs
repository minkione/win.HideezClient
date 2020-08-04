using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.IPC.DTO
{
    [DataContract]
    public class ProximitySettingsDTO
    {
        public ProximitySettingsDTO()
        {
        }

        [DataMember]
        public string SerialNo { get; set; }
        [DataMember]
        public string Mac { get; set; }
        [DataMember]
        public int LockProximity { get; set; }
        [DataMember]
        public int UnlockProximity { get; set; }
        [DataMember]
        public bool AllowEditProximitySettings { get; set; }
    }
}
