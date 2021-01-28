using System.Threading.Tasks;

namespace HideezClient.Modules.WorkstationManager
{
    interface IWorkstationManager
    {
        void ForceShutdown();
        void LockPC();
        void ActivateScreen();
    }
}