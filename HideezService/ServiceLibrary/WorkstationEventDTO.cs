using System;
using System.Runtime.Serialization;

namespace ServiceLibrary
{
    [DataContract]
    public class WorkstationEventDTO
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public int Event { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public string Note { get; set; }
        [DataMember]
        public string Computer { get; set; }
        [DataMember]
        public string UserSession { get; set; }
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public string AccountLogin { get; set; }
    }
}
