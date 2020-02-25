using Hideez.SDK.Communication.PasswordManager;
using HideezClient.Utilities;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HideezClient.Models
{
    public class Account
    {
        private readonly Device device;
        private readonly AccountRecord accountRecord;

        public Account(Device device, AccountRecord accountRecord)
        {
            this.device = device;
            this.accountRecord = accountRecord;
        }

        public string Id => $"{device.Id}:{accountRecord.Key}";
        public Device Device => device;
        public string Name => accountRecord.Name;

        public string[] Apps => AccountUtility.Split(accountRecord.Apps);
        public string[] Domains => AccountUtility.Split(accountRecord.Urls);

        public string Login => accountRecord.Login;
        public bool HasOtp => accountRecord.Flags.HasOtp;

        public async Task<string> TryGetPasswordAsync()
        {
            string password = null;

            try
            {
                if (device.IsConnected && device.IsInitialized)
                {
                    return await device.PasswordManager.GetPasswordAsync(accountRecord.Key);
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
                if (device.IsConnected && device.IsInitialized && HasOtp)
                {
                    otp = await device.PasswordManager.GetOtpAsync(accountRecord.Key);
                }
            }
            catch (Exception ex){ }

            return otp;
        }
    }
}
