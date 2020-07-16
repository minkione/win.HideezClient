using HideezMiddleware.IPC.Messages;
using HideezMiddleware.ScreenActivation;
using Meta.Lib.Modules.PubSub;
using ServiceLibrary.Implementation.ClientManagement;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.ScreenActivation
{
    class WcfScreenActivator : ScreenActivator
    {
        readonly ServiceClientSessionManager _sessionManager;
        readonly IMetaPubSub _messenger;

        public WcfScreenActivator(ServiceClientSessionManager sessionManager, IMetaPubSub messenger)
        {
            _sessionManager = sessionManager;
            _messenger = messenger;
        }

        public override void ActivateScreen()
        {
            Task.Run(async () =>
            {
                try
                {
                    await _messenger.Publish(new ActivateScreenRequestMessage());
                }
                catch (Exception) { }
            });
        }
    }
}
