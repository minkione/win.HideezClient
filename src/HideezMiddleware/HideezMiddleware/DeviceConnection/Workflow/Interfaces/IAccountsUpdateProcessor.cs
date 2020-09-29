using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IAccountsUpdateProcessor
    {
        /// <summary>
        /// Request server to update accounts on the device, if server is connected
        /// </summary>
        /// <param name="device">Vault that should be updated</param>
        /// <param name="vaultInfo">Vault info from server</param>
        /// <param name="onlyOsAccounts">If true, only os accounts will be updated. Else, only non os accounts will be updated</param>
        Task UpdateAccounts(IDevice device, HwVaultInfoFromHesDto vaultInfo, bool onlyOsAccounts);
    }
}
