using Hideez.SDK.Communication.Log;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System.Threading.Tasks;
using WinBle._10._0._18362;

namespace HideezMiddleware
{
    public sealed class UnpairProvider : Logger, IUnpairProvider
    {
        readonly IMetaPubSub _messenger;

        public UnpairProvider(IMetaPubSub messenger, ILog log)
            : base(nameof(UnpairProvider), log)
        {
            _messenger = messenger;
        }

        public async Task UnpairAsync(string id)
        {
            await _messenger?.Publish(new UnpairDeviceMessage(id));
        }
    }
}
