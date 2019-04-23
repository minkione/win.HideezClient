using System;

namespace ServiceLibrary.Implementation
{
    class ServiceClientSession
    {
        public ServiceClientSession(ICallbacks callbacks)
        {
            this.Callbacks = callbacks;
            Id = new Guid().ToString();
        }

        public ICallbacks Callbacks { get; }

        public string Id { get; }
    }
}
