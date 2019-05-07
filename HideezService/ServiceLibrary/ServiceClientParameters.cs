using System.Runtime.Serialization;

namespace ServiceLibrary
{
    [DataContract]
    public class ServiceClientParameters
    {
        [DataMember]
        public ClientType ClientType { get; set; }
    }
}
