using Hideez.SDK.Communication.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IUserAuthorizationProcessor
    {
        /// <summary>
        /// Perform user authorization on the vault
        /// </summary>
        /// <param name="device">Vault to authorize user on</param>
        /// <param name="ct">Cancellation token</param>
        /// <exception cref="OperationCanceledException">Thrown if cancellation token is cancelled.</exception>
        /// <exception cref="WorkflowException">Thrown if vault was locked due to too many incorrect pin attempts</exception>
        /// <exception cref="VaultFailedToAuthorizeException">Thrown at the end of operation user failed to authorize</exception>
        Task AuthorizeUser(IDevice device, CancellationToken ct);
    }
}
