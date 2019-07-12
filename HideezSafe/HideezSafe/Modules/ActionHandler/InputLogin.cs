using Hideez.ARM;
using Hideez.ISM;
using HideezMiddleware.Settings;
using HideezSafe.Models;
using HideezSafe.Models.Settings;
using HideezSafe.Modules.DeviceManager;
using HideezSafe.Modules.Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules.ActionHandler
{
    /// <summary>
    /// Implement input Login algothoritm
    /// </summary>
    class InputLogin : InputBase
    {
        public InputLogin(IInputHandler inputHandler, ITemporaryCacheAccount temporaryCacheAccount
                        , IInputCache inputCache, ISettingsManager<ApplicationSettings> settingsManager
                        , IWindowsManager windowsManager, IDeviceManager deviceManager)
                        : base(inputHandler, temporaryCacheAccount, inputCache, settingsManager, windowsManager, deviceManager)
        {
        }

        /// <summary>
        /// If account has login simulate input
        /// </summary>
        /// <param name="account">Account info about the login</param>
        /// <returns>True if PmAccountDTO has login</returns>
        protected override async Task<bool> InputAccountAsync(Account account)
        {
            if (account != null && !string.IsNullOrWhiteSpace(account.Login))
            {
                await SimulateInput(account.Login);
                SetCache(account);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Show dialog add new account
        /// </summary>
        /// <param name="appInfo">AppInfo for new account</param>
        /// <param name="devicesId">Devices on which the search was performed</param>
        protected override void OnAccountNotFoundError(AppInfo appInfo, string[] devicesId)
        {
            throw new LoginNotFoundException(string.Format(TranslationSource.Instance["Exception.LoginNotFound"], appInfo.Title), appInfo, devicesId);
        }
    }
}