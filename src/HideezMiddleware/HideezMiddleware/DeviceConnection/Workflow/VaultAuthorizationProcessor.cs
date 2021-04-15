using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection.Workflow.Interfaces;
using HideezMiddleware.Localize;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class VaultAuthorizationProcessor : Logger, IVaultAuthorizationProcessor
    {
        readonly IClientUiManager _ui;
        readonly IHesAppConnection _hesConnection;

        public VaultAuthorizationProcessor(IHesAppConnection hesConnection, IClientUiManager ui, ILog log)
            : base(nameof(VaultAuthorizationProcessor), log)
        {
            _hesConnection = hesConnection;
            _ui = ui;
        }

        public async Task AuthVault(IDevice device, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (device.AccessLevel.IsMasterKeyRequired && _hesConnection.State == HesConnectionState.Connected)
            {
                await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.MasterKey.AwaitingHESAuth"], device.Id);

                await _hesConnection.AuthHwVault(device.SerialNo);

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
