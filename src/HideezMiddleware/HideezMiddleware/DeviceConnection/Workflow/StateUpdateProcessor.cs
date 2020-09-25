using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
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
            if (vaultInfo.NeedStateUpdate)
            {
                vaultInfo = await _hesConnection.UpdateDeviceState(device, ct);
                await device.RefreshDeviceInfo();
            }
            
            return vaultInfo;
        }
    }
}
