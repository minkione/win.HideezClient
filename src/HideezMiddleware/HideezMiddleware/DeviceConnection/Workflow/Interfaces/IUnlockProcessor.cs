using Hideez.SDK.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IUnlockProcessor
    {
        Task UnlockWorkstation(IDevice device, string flowId, Action<WorkstationUnlockResult> onUnlockAttempt, CancellationToken ct);
    }
}
