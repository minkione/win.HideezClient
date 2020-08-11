using System.Threading.Tasks;

namespace HideezClient.Modules
{
    interface IWorkstationManager
    {
        void ForceShutdown();
        void LockPC();
        void ActivateScreen();
    }
}