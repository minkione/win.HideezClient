using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Modules.ActionHandler
{

    [Serializable]
    public class FieldNotSecureException : System.Exception
    {
        public FieldNotSecureException() { }
        public FieldNotSecureException(string message) : base(message) { }
        public FieldNotSecureException(string message, System.Exception inner) : base(message, inner) { }
        protected FieldNotSecureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
