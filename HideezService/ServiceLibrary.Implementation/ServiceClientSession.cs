using System;
using System.Collections.Generic;

namespace ServiceLibrary.Implementation
{
    class ServiceClientSession
    {
        public ServiceClientSession(ClientType type, ICallbacks callbacks)
        {
            ClientType = type;
            Callbacks = callbacks;
            Id = Guid.NewGuid().ToString();
        }

        public ClientType ClientType { get; set; }

        public ICallbacks Callbacks { get; }

        public string Id { get; }

        public IDictionary<string, bool> IsEnabledPropertyMonitoring { get; } = new Dictionary<string, bool>();
        public IDictionary<string, bool> IsEnabledProximityMonitoring { get; } = new Dictionary<string, bool>();
    }
}
