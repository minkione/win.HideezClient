using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Modules
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
