using System.Threading.Tasks;

namespace HideezClient.Modules.ActionHandler
{
    interface IInputAlgorithm
    {
        Task InputAsync(string[] devicesId);
    }
}
