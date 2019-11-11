using System.Runtime.Serialization;

namespace ServiceLibrary
{
    public enum ClientType
    {
        ServiceHost,
        DesktopClient,
        TestConsole,
        RemoteDeviceConnection,
    }

    [DataContract]
    public class ServiceClientParameters
    {
        [DataMember]
        public ClientType ClientType { get; set; }
    }
}
