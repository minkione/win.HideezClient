using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IActivationProcessor
    {
        Task<HwVaultInfoFromHesDto> ActivateVault(IDevice device, HwVaultInfoFromHesDto vaultInfo, CancellationToken ct);
    }
}
