using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection.Workflow.Interfaces;
using HideezMiddleware.Localize;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class AccountsUpdateProcessor : Logger, IAccountsUpdateProcessor
    {
        readonly IHesAppConnection _hesConnection;

        public AccountsUpdateProcessor(IHesAppConnection hesConnection, ILog log)
            : base(nameof(AccountsUpdateProcessor), log)
        {
            _hesConnection = hesConnection;
        }

        public async Task UpdateAccounts(IDevice device, HwVaultInfoFromHesDto vaultInfo, bool onlyOsAccounts)
        {
            if (_hesConnection.State == HesConnectionState.Connected)
            {
                if ((vaultInfo.NeedUpdateOSAccounts && onlyOsAccounts) || (vaultInfo.NeedUpdateNonOSAccounts && !onlyOsAccounts))
                {
                    try
                    {
                        await _hesConnection.UpdateHwVaultAccounts(device.SerialNo, onlyOsAccounts);
                        await device.RefreshDeviceInfo();
                    }
                    catch (HesException ex) when (ex.ErrorCode == HideezErrorCode.ERR_LICENSE_EXPIRED)
                    {
                        WriteLine($"Accounts upload cancelled, vault license expired {string.Format(TranslationSource.Instance["ConnectionFlow.VaultSerialNo"], device.SerialNo)}", LogErrorSeverity.Warning);
                    }
                }
            }
        }
    }
}
