using Hideez.SDK.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IVaultConnectionProcessor
    {
        Task<IDevice> ConnectVault(string mac, bool rebondOnFail, CancellationToken ct);
        Task WaitVaultInitialization(string mac, IDevice device, CancellationToken ct);
    }
}
