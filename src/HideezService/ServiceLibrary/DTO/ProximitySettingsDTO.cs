using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary
{
    [DataContract]
    public class ProximitySettingsDTO
    {
        [DataMember]
        public string SerialNo { get; set; }
        [DataMember]
        public string Mac { get; set; }
        [DataMember]
        public int LockProximity { get; set; }
        [DataMember]
        public int UnlockProximity { get; set; }
    }
}
