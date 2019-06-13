using System.Threading.Tasks;

namespace HideezSafe.Modules.ActionHandler
{
    interface IInputAlgorithm
    {
        Task InputAsync(string[] devicesId);
    }
}
