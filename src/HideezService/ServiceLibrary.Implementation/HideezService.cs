using System.Collections.Concurrent;
using HideezMiddleware.Modules;

namespace ServiceLibrary.Implementation
{
    public partial class HideezService
    {
        public ConcurrentBag<IModule> ServiceModules { get; } = new ConcurrentBag<IModule>();
        
        // Hard reference to the DI container used to build service
        public object Container { get; }

        public HideezService(object container)
        {
            // The container is preserved to ensure that "singleton" service modules resolved by the container
            // are not disposed when container is discarded
            Container = container;
        }
    }
}
