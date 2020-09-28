using Hideez.SDK.Communication.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IUserAuthorizationProcessor
    {
        Task AuthorizeUser(IDevice device, CancellationToken ct);
    }
}
