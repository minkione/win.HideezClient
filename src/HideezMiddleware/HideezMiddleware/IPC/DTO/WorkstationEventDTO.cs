using System;
using System.Runtime.Serialization;

namespace HideezMiddleware.IPC.DTO
{
    [DataContract]
    public class WorkstationEventDTO
    {
        public WorkstationEventDTO()
        {
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public DateTime Date { get; set; }
        [DataMember]
        public DateTime LocalDateTime { get; set; }
        [DataMember]
        public TimeZoneInfo TimeZone { get; set; }
        [DataMember]
        public int EventId { get; set; }
        [DataMember]
        public int Severity { get; set; }
        [DataMember]
        public string Note { get; set; }
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public string AccountName { get; set; }
        [DataMember]
        public string AccountLogin { get; set; }
    }
}
