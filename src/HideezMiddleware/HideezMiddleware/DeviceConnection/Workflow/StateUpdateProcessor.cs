using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.Localize;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class StateUpdateProcessor : Logger
    {
        readonly UiProxyManager _ui;
        readonly IHesAppConnection _hesConnection;

        public StateUpdateProcessor(IHesAppConnection hesConnection, UiProxyManager ui, ILog log)
            : base(nameof(StateUpdateProcessor), log)
        {
            _hesConnection = hesConnection;
            _ui = ui;
        }

        public async Task<HwVaultInfoFromHesDto> UpdateDeviceState(IDevice device, HwVaultInfoFromHesDto vaultInfo, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (vaultInfo.NeedStateUpdate && _hesConnection.State == HesConnectionState.Connected)
            {
                vaultInfo = await _hesConnection.UpdateDeviceState(device, ct);
                await device.RefreshDeviceInfo();
            }

            if (device.AccessLevel.IsLinkRequired)
            {
                if (_hesConnection.State == HesConnectionState.Connected)
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.StateUpdate.Error.NotAssignedToUser"]);
                else
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.StateUpdate.Error.CannotLinkToUser"]);
            }

            return vaultInfo;
        }
    }
}
