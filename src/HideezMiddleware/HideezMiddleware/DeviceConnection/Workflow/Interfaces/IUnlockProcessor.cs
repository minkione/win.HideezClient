using Hideez.SDK.Communication.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IUnlockProcessor
    {
        /// <summary>
        /// Unlock workstation using os account from target device
        /// </summary>
        /// <param name="device">Vault with os account</param>
        /// <param name="flowId">Mainworkflow id</param>
        /// <param name="onUnlockAttempt">Delegate that is invoked after unlock attemt</param>
        /// <param name="ct">Cancellation token</param>
        /// <exception cref="WorkstationUnlockFailedException">Thrown if workstation unlock failed</exception>
        /// <exception cref="OperationCanceledException">Thrown if cancellation token is cancelled.</exception>
        Task UnlockWorkstation(IDevice device, string flowId, Action<WorkstationUnlockResult> onUnlockAttempt, CancellationToken ct);
    }
}
