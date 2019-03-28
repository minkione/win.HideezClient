using ServiceLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceLibrary.Implementation
{
    class ServiceClientSessionManager
    {
        readonly List<ServiceClientSession> sessions = new List<ServiceClientSession>();

        public ServiceClientSessionManager()
        {
        }

        internal ServiceClientSession Add(ICallbacks callbacks)
        {
            var session = new ServiceClientSession(callbacks);
            lock (sessions)
            {
                sessions.Add(session);
            }
            return session;
        }

        internal void Remove(ServiceClientSession session)
        {
            lock (sessions)
            {
                sessions.Remove(session);
            }
        }

        ServiceClientSession GetSessionById(string id)
        {
            lock (sessions)
            {
                return sessions.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
