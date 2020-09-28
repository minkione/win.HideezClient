using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IStateUpdateProcessor
    {
        Task<HwVaultInfoFromHesDto> UpdateDeviceState(IDevice device, HwVaultInfoFromHesDto vaultInfo, CancellationToken ct);
    }
}
