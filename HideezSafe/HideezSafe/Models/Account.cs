using Hideez.SDK.Communication.PasswordManager;
using System;

namespace HideezSafe.Models
{
    public class Account
    {
        private readonly AccountRecord accountRecord;

        public Account(string deviceId, AccountRecord accountRecord)
        {
            DeviceId = deviceId;
            this.accountRecord = accountRecord;
        }

        public string DeviceId { get; }
        public string Name => accountRecord.Name;

        public string[] Apps => SplitAppsToLines(accountRecord.Apps);
        public string[] Urls => SplitAppsToLines(accountRecord.Urls);

        public string Login => accountRecord.Login;
        public string Password => accountRecord.Password;
        public bool HasOtpSecret => !string.IsNullOrWhiteSpace(accountRecord.OtpSecret);
        public string OtpSecret => accountRecord.OtpSecret;

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
