using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Utilities;
using System;
using System.Threading.Tasks;

namespace HideezClient.Models
{
    public class AccountModel
    {
        private readonly IVaultModel vault;
        private readonly AccountRecord accountRecord;

        public AccountModel(IVaultModel device, AccountRecord accountRecord)
        {
            this.vault = device;
            this.accountRecord = accountRecord;
        }

        public string Id => $"{vault.Id}:{accountRecord.Key}";
        public IVaultModel Vault => vault;
        public string Name => accountRecord.Name;

        public string[] Apps => AccountUtility.Split(accountRecord.Apps);
        public string[] Domains => AccountUtility.Split(accountRecord.Urls);

        public string Login => accountRecord.Login;
        public bool HasOtp => accountRecord.Flags.HasOtp;

        public bool IsReadOnly => accountRecord.Flags.IsReadOnly;

        public async Task<string> TryGetPasswordAsync()
        {
            string password = null;

            try
            {
                if (vault.IsConnected && vault.IsInitialized)
                {
                    return await vault.PasswordManager.GetPasswordAsync(accountRecord.Key);
                }
            }
            catch { }

            return password;
        }

        public async Task<string> TryGetOptAsync()
        {
            string otp = null;
            try
            {
                if (vault.IsConnected && vault.IsInitialized && HasOtp)
                {
                    otp = await vault.PasswordManager.GetOtpAsync(accountRecord.Key);
                }
            }
            catch (Exception ex){ }

            return otp;
        }
    }
}
