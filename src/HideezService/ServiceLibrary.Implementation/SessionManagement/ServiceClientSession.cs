using System;

namespace ServiceLibrary.Implementation.SessionManagement
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
    }
}
