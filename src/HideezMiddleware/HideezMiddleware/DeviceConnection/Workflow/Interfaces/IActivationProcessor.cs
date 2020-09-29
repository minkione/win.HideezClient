using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IActivationProcessor
    {
        /// <summary>
        /// Try to activate a vault that can be unlocked. Throws <see cref="WorkflowException"/> if vault cannot be unlocked.
        /// </summary>
        /// <param name="device">Vault that should be activated</param>
        /// <param name="vaultInfo">Vault info from server</param>
        /// <param name="ct">Cancellation token</param>
        /// <exception cref="WorkflowException">
        /// Thrown if:
        /// - Vault was locked due to too many incorrect activation code entries.
        /// - Vault cannot be unlocked
        /// - Vault is locked at the end of operation
        /// </exception>
        /// <exception cref="OperationCanceledException">Thrown if cancellation token is cancelled.</exception>
        /// <returns>Returns updated <see cref="HwVaultInfoFromHesDto"/> after successfull activation.</returns>
        Task<HwVaultInfoFromHesDto> ActivateVault(IDevice device, HwVaultInfoFromHesDto vaultInfo, CancellationToken ct);
    }
}
