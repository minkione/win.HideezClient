using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLibrary.Implementation.SessionManagement
{
    class ServiceClientSessionManager
    {
        readonly object sessionsLock = new object();
        List<ServiceClientSession> _sessions = new List<ServiceClientSession>();

        public event EventHandler<ServiceClientSession> SessionClosed;
        public event EventHandler<ServiceClientSession> SessionAdded;

        public IReadOnlyCollection<ServiceClientSession> Sessions
        {
            get
            {
                return _sessions.ToList();
            }
        }

        public ServiceClientSessionManager()
        {
        }

        internal ServiceClientSession Add(ClientType type, ICallbacks callbacks)
        {
            var session = new ServiceClientSession(type, callbacks);
            lock (sessionsLock)
            {
                _sessions.Add(session);
            }
            SessionAdded?.Invoke(this, new ServiceClientSession(type, callbacks));
            return session;
        }

        internal void Remove(ServiceClientSession session)
        {
            lock (sessionsLock)
            {
                _sessions.Remove(session);
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
