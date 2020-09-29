using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System.Threading;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface ICacheVaultInfoProcessor
    {
        /// <summary>
        /// Cache and load into vault additional metadata properties. If info for server is not available, metadata is loaded from cache instead.
        /// </summary>
        /// <param name="device">Vault to update metadata</param>
        /// <param name="dto">Vault info from server</param>
        /// <param name="ct">Cancellation token</param>
        /// <exception cref="OperationCanceledException">Thrown if cancellation token is cancelled.</exception>
        void CacheAndUpdateVaultOwner(ref IDevice device, HwVaultInfoFromHesDto dto, CancellationToken ct);
    }
}
