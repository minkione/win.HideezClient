using System;
using System.Runtime.Serialization;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    internal class VaultFailedToAuthorizeException : WorkflowException
    {
        public VaultFailedToAuthorizeException()
        {
        }

        public VaultFailedToAuthorizeException(string message) : base(message)
        {
        }

        public VaultFailedToAuthorizeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VaultFailedToAuthorizeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
