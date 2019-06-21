using System.Runtime.Serialization;

namespace ServiceLibrary
{
    [DataContract]
    public class HideezServiceFault
    {
        public HideezServiceFault(string msg, int code)
        {
            FaultMessage = msg;
            ErrorCode = code;
        }

        [DataMember]
        public string FaultMessage;
        [DataMember]
        public int ErrorCode;
    }
}
