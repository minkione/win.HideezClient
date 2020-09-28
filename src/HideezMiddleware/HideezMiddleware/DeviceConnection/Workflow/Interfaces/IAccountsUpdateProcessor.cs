using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IAccountsUpdateProcessor
    {
        Task UpdateAccounts(IDevice device, HwVaultInfoFromHesDto vaultInfo, bool onlyOsAccounts);
    }
}
