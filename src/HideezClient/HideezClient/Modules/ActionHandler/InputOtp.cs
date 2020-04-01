using Hideez.ARM;
using Hideez.ISM;
using HideezMiddleware.Settings;
using HideezClient.Models;
using HideezClient.Models.Settings;
using HideezClient.Modules.VaultManager;
using HideezClient.Modules.Localize;
using System.Linq;
using System.Threading.Tasks;
using System;
using Hideez.SDK.Communication;

namespace HideezClient.Modules.ActionHandler
{
    /// <summary>
    /// Implement input OTP algorithm
    /// </summary>
    class InputOtp : InputBase
    {
        readonly IEventPublisher _eventPublisher;

        public InputOtp(IInputHandler inputHandler, ITemporaryCacheAccount temporaryCacheAccount
                        , IInputCache inputCache, ISettingsManager<ApplicationSettings> settingsManager
                        , IWindowsManager windowsManager, IVaultManager deviceManager
                        , IEventPublisher eventPublisher)
                        : base(inputHandler, temporaryCacheAccount, inputCache, settingsManager, windowsManager, deviceManager)
        {
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Get OTP from service by account deviceId and key and simulate input
        /// </summary>
        /// <param name="account">Account info about the OTP</param>
        /// <returns>True if found data for OTP</returns>
        protected override async Task<bool> InputAccountAsync(AccountModel account)
        {
            string otpSecret = await account?.TryGetOptAsync();
            if (!string.IsNullOrEmpty(otpSecret))
            {
                await SimulateInput(otpSecret);
                SimulateEnter();
                temporaryCacheAccount.Clear();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Throw InputException "No One Account With OTP"
        /// </summary>
        protected override void OnAccountNotFoundError(AppInfo appInfo, string[] devicesId)
        {
            throw new OtpNotFoundException(string.Format(TranslationSource.Instance["Exception.NoOneAccountWithOtp"], appInfo.Title), appInfo, devicesId);
        }

        /// <summary>
        /// Filtering intersect accounts and devices and previous input
        /// </summary>
        /// <param name="accounts">Found accounts on devices</param>
        /// <param name="devicesId">Devices for filtering</param>
        /// <returns>Filtered accounts</returns>
        protected override AccountModel[] FilterAccounts(AccountModel[] accounts, string[] devicesId)
        {
            var filterAccounts = base.FilterAccounts(accounts, devicesId)
                .Where(a => a.HasOtp).ToArray();

            return FindValueForPreviousInput(filterAccounts, temporaryCacheAccount.OtpReqCache.Value);
        }

        protected override async void OnAccountEntered(AppInfo appInfo, AccountModel account)
        {
            base.OnAccountEntered(appInfo, account);

            await _eventPublisher.PublishEventAsync(new HideezServiceReference.WorkstationEventDTO
            {
                Id = Guid.NewGuid().ToString(),
                Date = DateTime.UtcNow,
                AccountLogin = account.Login,
                AccountName = account.Name,
                DeviceId = account.Vault.SerialNo,
                EventId = (int)WorkstationEventType.CredentialsUsed_Otp,
                Note = appInfo.Title,
                Severity = (int)WorkstationEventSeverity.Info,
            });
        }
    }
}
