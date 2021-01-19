using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace HideezMiddleware.Utils.WorkstationHelper
{
    public interface IWorkstationHelper
    {
        PhysicalAddress GetCurrentMAC(IPAddress localIPAddres);
        Task<IPAddress> GetLocalIPAddressAsync(IPEndPoint endPoint);
        Task<string[]> GetAllUserNamesAsync();
        string[] GetAllUserNames();
        string GetSessionName(uint sessionId);
        uint GetSessionId();
        WorkstationInformationHelper.LockState GetSessionLockState(uint session_id);
        bool IsSessionLocked(uint session_id);
        WorkstationInformationHelper.LockState GetActiveSessionLockState();
        bool IsActiveSessionLocked();
        WorkstationInformationHelper.LockState GetCurrentSessionLockState();
        bool IsCurrentSessionLocked();
    }
}
