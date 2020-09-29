using Hideez.SDK.Communication.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IVaultAuthorizationProcessor
    {
        /// <summary>
        /// Request server to authorize vault on this workstation
        /// </summary>
        /// <param name="device">Vault to authorize</param>
        /// <param name="ct">Cancellation token</param>
        /// <exception cref="OperationCanceledException">Thrown if cancellation token is cancelled.</exception>
        /// <exception cref="WorkflowException">Thrown if vault is not authorized at the end of operation.</exception>
        Task AuthVault(IDevice device, CancellationToken ct);
    }
}
