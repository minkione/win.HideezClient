using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class AccountsUpdateProcessor : Logger
    {
        IHesAppConnection _hesConnection;

        public AccountsUpdateProcessor(IHesAppConnection hesConnection, ILog log)
            : base(nameof(AccountsUpdateProcessor), log)
        {
            _hesConnection = hesConnection;
        }

        public async Task UpdateAccounts(IDevice device, HwVaultInfoFromHesDto vaultInfo, bool onlyOsAccounts)
        {
            if ((vaultInfo.NeedUpdateOSAccounts && onlyOsAccounts) || (vaultInfo.NeedUpdateNonOSAccounts && !onlyOsAccounts))
            {
                await _hesConnection.UpdateAccounts(device.SerialNo, onlyOsAccounts);
                await device.RefreshDeviceInfo();
            }
        }
    }
}
