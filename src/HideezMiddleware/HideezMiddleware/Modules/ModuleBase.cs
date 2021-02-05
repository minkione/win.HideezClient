using Hideez.SDK.Communication.Log;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules
{
    public abstract class ModuleBase : Logger, IModule
    {
        readonly protected IMetaPubSub _messenger;

        protected ModuleBase(IMetaPubSub messenger, string source, ILog log) 
            : base(source, log)
        {
            _messenger = messenger;
        }

        protected async Task SafePublish(IPubSubMessage message, bool logError = false)
        {
            try
            {
                await _messenger.Publish(message);
            }
            catch (Exception ex)
            {
                if (logError)
                    WriteLine(ex);
            }
        }
    }
}
