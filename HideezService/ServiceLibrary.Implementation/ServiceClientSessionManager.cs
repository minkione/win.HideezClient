using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLibrary.Implementation
{
    class ServiceClientSessionManager
    {
        readonly object sessionsLock = new object();

        public event EventHandler<ServiceClientSession> SessionClosed;

        public IReadOnlyCollection<ServiceClientSession> Sessions { get; } = new List<ServiceClientSession>();

        public ServiceClientSessionManager()
        {
        }

        internal ServiceClientSession Add(ClientType type, ICallbacks callbacks)
        {
            var session = new ServiceClientSession(type, callbacks);
            lock (sessionsLock)
            {
                (Sessions as List<ServiceClientSession>).Add(session);
            }
            return session;
        }

        internal void Remove(ServiceClientSession session)
        {
            lock (sessionsLock)
            {
                (Sessions as List<ServiceClientSession>).Remove(session);
            }
            SessionClosed?.Invoke(this, session);
        }

        ServiceClientSession GetSessionById(string id)
        {
            lock (sessionsLock)
            {
                return Sessions.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
