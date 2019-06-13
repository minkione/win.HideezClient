using Hideez.ARS;
using Hideez.ISM;
using HideezSafe.Models.Settings;
using HideezSafe.Modules.Localize;
using HideezSafe.Modules.SettingsManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules.ActionHandler
{
    /// <summary>
    /// Implement input OTP algorithm
    /// </summary>
    class InputOtp : InputBase
    {
        public InputOtp(IInputHandler inputHandler, ITemporaryCacheAccount temporaryCacheAccount
                        , IInputCache inputCache, ISettingsManager<ApplicationSettings> settingsManager
                        , IWindowsManager windowsManager)
                        : base(inputHandler, temporaryCacheAccount, inputCache, settingsManager, windowsManager)
        {
        }

        /// <summary>
        /// Get OTP from service by account deviceId and key and simulate input
        /// </summary>
        /// <param name="account">Account info about the OTP</param>
        /// <returns>True if found data for OTP</returns>
        protected override async Task<bool> InputAccountAsync(Account account)
        {
            if (account != null && account.HasOtpSecret)
            {
                string otp = await GetOtpAsync(account);
                if (otp != null)
                {
                    await SimulateInput(otp);
                    await SimulateEnterAsync();
                    SetCache(account);
                    return true;
                }
            }

            return false;
        }

        private Task<string> GetOtpAsync(Account account)
        {
            // TODO: get otp
            throw new NotImplementedException();
        }

        /// <summary>
        /// Throw InputException "No One Account With OTP"
        /// </summary>
        protected override void NotFoundAccounts(AppInfo appInfo, string[] devicesId)
        {
            throw new OtpNotFoundException(string.Format(TranslationSource.Instance["Exception.NoOneAccountWithOtp"], appInfo.Title), appInfo, devicesId);
        }

        /// <summary>
        /// Filtering intersect accounts and devices and previous input
        /// </summary>
        /// <param name="accounts">Found accounts on devices</param>
        /// <param name="devicesId">Devices for filtering</param>
        /// <returns>Filtered accounts</returns>
        protected override Account[] FilterAccounts(Account[] accounts, string[] devicesId)
        {
            var filterAccounts = base.FilterAccounts(accounts, devicesId)
                .Where(a => a.HasOtpSecret).ToArray();

            return FindValueForPreviousInput(filterAccounts, temporaryCacheAccount.OtpReqCache.Value);
        }
    }
}
