using HideezMiddleware.IPC.Messages;
using HideezMiddleware.ScreenActivation;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace ServiceLibrary.Implementation.ScreenActivation
{
    class MetalibScreenActivator : ScreenActivator
    {
        readonly IMetaPubSub _messenger;

        public MetalibScreenActivator(IMetaPubSub messenger)
        {
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
