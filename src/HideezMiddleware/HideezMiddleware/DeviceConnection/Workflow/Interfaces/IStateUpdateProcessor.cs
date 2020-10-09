using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IStateUpdateProcessor
    {
        /// <summary>
        /// Request server to update vault state (Link, Wipe, Unlock), if server is connected. 
        /// Throws <see cref="WorkflowException"/> if vault is not assigned to any user at the end of operation.
        /// </summary>
        /// <param name="device">Vault to update state</param>
        /// <param name="vaultInfo">Vault info from server</param>
        /// <param name="ct">Cancellation token</param>
        /// <exception cref="WorkflowException">Thrown when vault is not assigned to any user at the end of operation</exception>
        /// <exception cref="HideezException">Thrown when vault is wiped or an error occurs on the server</exception>
        /// <exception cref="OperationCanceledException">Thrown if cancellation token is cancelled.</exception>
        /// <returns>Returns updated <see cref="HwVaultInfoFromHesDto"/> after successfull link or unlock</returns>
        Task<HwVaultInfoFromHesDto> UpdateVaultStatus(IDevice device, HwVaultInfoFromHesDto vaultInfo, CancellationToken ct);
    }
}
