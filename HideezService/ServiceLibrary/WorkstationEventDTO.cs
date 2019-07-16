using System;
using System.Runtime.Serialization;

namespace ServiceLibrary
{
    [DataContract]
    public class WorkstationEventDTO
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public int Event { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public object Note { get; set; }
        [DataMember]
        public string Computer { get; set; }
        [DataMember]
        public string UserSession { get; set; }
        [DataMember]
        public string DeviceSN { get; set; }
        [DataMember]
        public string AccountName { get; set; }
    }
}
