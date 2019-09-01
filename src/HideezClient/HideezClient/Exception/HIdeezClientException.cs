using System;

namespace HideezClient
{
    [Serializable]
    public class HideezClientException : Exception
    {
        public HideezClientException() { }
        public HideezClientException(string message) : base(message) { }
        public HideezClientException(string message, Exception inner) : base(message, inner) { }
        protected HideezClientException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
