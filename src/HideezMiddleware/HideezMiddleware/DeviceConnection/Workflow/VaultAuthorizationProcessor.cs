using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Tasks;
using HideezMiddleware.Localize;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class VaultAuthorizationProcessor : Logger
    {
        readonly UiProxyManager _ui;
        readonly IHesAppConnection _hesConnection;

        public VaultAuthorizationProcessor(IHesAppConnection hesConnection, UiProxyManager ui, ILog log)
            : base(nameof(VaultAuthorizationProcessor), log)
        {
            _hesConnection = hesConnection;
            _ui = ui;
        }

        public async Task ActivateVault(IDevice device, CancellationToken ct)
        {
            if (!device.AccessLevel.IsMasterKeyRequired)
                return;
            
            ct.ThrowIfCancellationRequested();

            await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.MasterKey.AwaitingHESAuth"], device.Mac);

            await _hesConnection.AuthDevice(device.SerialNo);

            await new WaitVaultAuthProc(device).Run(SdkConfig.SystemStateEventWaitTimeout, ct);
        }
    }
}
