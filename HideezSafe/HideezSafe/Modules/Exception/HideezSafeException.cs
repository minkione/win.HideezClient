using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    [Serializable]
    public class HideezSafeException : Exception
    {
        public HideezSafeException() { }
        public HideezSafeException(string message) : base(message) { }
        public HideezSafeException(string message, Exception inner) : base(message, inner) { }
        protected HideezSafeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
