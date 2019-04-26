using System;

namespace ServiceLibrary.Implementation
{
    class ServiceClientSession
    {
        public ServiceClientSession(ICallbacks callbacks)
        {
            this.Callbacks = callbacks;
            Id = Guid.NewGuid().ToString();
        }

        public ICallbacks Callbacks { get; }

        public string Id { get; }
    }
}
