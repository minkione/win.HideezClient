using System;
using System.Runtime.Serialization;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    internal class WorkstationUnlockFailedException : WorkflowException
    {
        public WorkstationUnlockFailedException()
        {
        }

        public WorkstationUnlockFailedException(string message) : base(message)
        {
        }

        public WorkstationUnlockFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WorkstationUnlockFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
