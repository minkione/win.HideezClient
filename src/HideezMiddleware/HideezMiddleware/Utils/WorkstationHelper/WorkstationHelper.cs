using Hideez.SDK.Communication.Log;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace HideezMiddleware.Utils.WorkstationHelper
{
    public class WorkstationHelper : IWorkstationHelper
    {
        public WorkstationHelper(ILog log)
        {
            WorkstationInformationHelper.Log = log;
        }

        public WorkstationInformationHelper.LockState GetActiveSessionLockState()
        {
            return WorkstationInformationHelper.GetActiveSessionLockState();
        }

        public string[] GetAllUserNames()
        {
            return WorkstationInformationHelper.GetAllUserNames();
        }

        public Task<string[]> GetAllUserNamesAsync()
        {
            return WorkstationInformationHelper.GetAllUserNamesAsync();
        }

        public PhysicalAddress GetCurrentMAC(IPAddress localIPAddres)
        {
            return WorkstationInformationHelper.GetCurrentMAC(localIPAddres);
        }

        public WorkstationInformationHelper.LockState GetCurrentSessionLockState()
        {
            return WorkstationInformationHelper.GetCurrentSessionLockState();
        }

        public Task<IPAddress> GetLocalIPAddressAsync(IPEndPoint endPoint)
        {
            return WorkstationInformationHelper.GetLocalIPAddressAsync(endPoint);
        }

        public uint GetSessionId()
        {
            return WorkstationInformationHelper.GetSessionId();
        }

        public WorkstationInformationHelper.LockState GetSessionLockState(uint session_id)
        {
            return WorkstationInformationHelper.GetSessionLockState(session_id);
        }

        public string GetSessionName(uint sessionId)
        {
            return WorkstationInformationHelper.GetSessionName(sessionId);
        }

        public bool IsActiveSessionLocked()
        {
            return WorkstationInformationHelper.IsActiveSessionLocked();
        }

        public bool IsCurrentSessionLocked()
        {
            return WorkstationInformationHelper.IsCurrentSessionLocked();
        }

        public bool IsSessionLocked(uint session_id)
        {
            return WorkstationInformationHelper.IsSessionLocked(session_id);
        }
    }
}
