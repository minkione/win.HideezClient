using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface ILicensingProcessor
    {
        /// <summary>
        /// Upload new licenses to the vault and ensure it has at least one license. 
        /// Throws <see cref="WorkflowException"/> if vault has no licenses when operation is finished
        /// </summary>
        /// <param name="device">Vault to upload licenses too</param>
        /// <param name="vaultInfo">Vault info from server</param>
        /// <param name="ct">Cancellation token</param>
        /// <exception cref="WorkflowException">Thrown if:
        /// - Server sent license with incorrect data
        /// - Vault has no licenses at the end of operation
        /// </exception>
        /// <exception cref="OperationCanceledException">Thrown if cancellation token is cancelled.</exception>
        Task CheckLicense(IDevice device, HwVaultInfoFromHesDto vaultInfo, CancellationToken ct);
    }
}
