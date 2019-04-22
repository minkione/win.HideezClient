using System;

namespace ServiceLibrary
{
    /// <summary>
    /// This service is used to provide automatic contract generation for other projects
    /// All interface members of IHideezService can be left as not implemented
    /// </summary>
    class ContractProviderService : IHideezService
    {
        public bool AttachClient(ServiceClientParameters parameters)
        {
            throw new NotImplementedException();
        }

        public void DetachClient()
        {
            throw new NotImplementedException();
        }

        public int Ping()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }
    }
}
