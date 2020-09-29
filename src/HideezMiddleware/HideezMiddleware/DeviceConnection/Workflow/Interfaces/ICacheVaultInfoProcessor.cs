using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System.Threading;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface ICacheVaultInfoProcessor
    {
        void CacheAndUpdateVaultOwner(ref IDevice device, HwVaultInfoFromHesDto dto, CancellationToken ct);
    }
}
