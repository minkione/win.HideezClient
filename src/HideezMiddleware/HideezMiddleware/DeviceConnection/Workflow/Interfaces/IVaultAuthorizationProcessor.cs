using Hideez.SDK.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IVaultAuthorizationProcessor
    {
        Task AuthVault(IDevice device, CancellationToken ct);
    }
}
