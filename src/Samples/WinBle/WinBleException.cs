using System;
using System.Runtime.Serialization;

namespace WinBle
{
    internal class WinBleException : Exception
    {
        public WinBleException()
        {
        }

        public WinBleException(string message) : base(message)
        {
        }

        public WinBleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WinBleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
