using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Tasks;
using HideezMiddleware.DeviceConnection.Workflow.Interfaces;
using HideezMiddleware.Localize;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class VaultAuthorizationProcessor : Logger, IVaultAuthorizationProcessor
    {
        readonly UiProxyManager _ui;
        readonly IHesAppConnection _hesConnection;

        public VaultAuthorizationProcessor(IHesAppConnection hesConnection, UiProxyManager ui, ILog log)
            : base(nameof(VaultAuthorizationProcessor), log)
        {
            _hesConnection = hesConnection;
            _ui = ui;
        }

        public async Task AuthVault(IDevice device, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (device.AccessLevel.IsMasterKeyRequired)
            {
                if (_hesConnection.State != HesConnectionState.Connected)
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.MasterKey.Error.AuthFailedNoNetwork"]);

                await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.MasterKey.AwaitingHESAuth"], device.Mac);

                await _hesConnection.AuthDevice(device.SerialNo);

                await new WaitMasterKeyProc(device).Run(SdkConfig.SystemStateEventWaitTimeout, ct);

                await device.RefreshDeviceInfo();
            }

            if (device.AccessLevel.IsMasterKeyRequired)
            {
                if (_hesConnection.State == HesConnectionState.Connected)
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.MasterKey.Error.AuthFailed"]);
                else
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.MasterKey.Error.AuthFailedNoNetwork"]);
            }
        }
    }
}
