using Hideez.SDK.Communication.PasswordManager;
using System;
using System.Threading.Tasks;

namespace HideezSafe.Models
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

        public string Id => $"{DeviceId}:{accountRecord.Key}";
        public string DeviceId => device.Id;
        public string Name => accountRecord.Name;

        public string[] Apps => SplitAppsToLines(accountRecord.Apps);
        public string[] Urls => SplitAppsToLines(accountRecord.Urls);

        public string Login => accountRecord.Login;
        public bool HasOtp => accountRecord.HasOtp;

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

        public async Task<string> TryGetOptSecretAsync()
        {
            throw new NotImplementedException("Not implemented Otp secret.");
            string otpSecret = null;
            try
            {
                if (device.IsConnected && device.IsInitialized && HasOtp)
                {
                    // TODO: Implemented Otp
                }
            }
            catch { }

            return otpSecret;
        }

        public static string[] SplitAppsToLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            string[] splitChars = { "\r\n", "\n", "\r" };
            string[] words = text.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length; i++)
                words[i] = words[i].Trim();

            return words;
        }
    }
}
