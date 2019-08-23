using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe
{
    [Serializable]
    public class HIdeezSafeException : Exception
    {
        public HIdeezSafeException() { }
        public HIdeezSafeException(string message) : base(message) { }
        public HIdeezSafeException(string message, System.Exception inner) : base(message, inner) { }
        protected HIdeezSafeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
