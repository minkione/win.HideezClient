using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Microsoft.Win32;
using System;

namespace HideezMiddleware.UnlockToken
{
    public class UnlockTokenProvider : Logger, IUnlockTokenProvider
    {
        const string REG_UNLOCK_TOKEN_NAME = "unlock_token";
        readonly RegistryKey _registryRootKey;

        public UnlockTokenProvider(RegistryKey rootKey, ILog log)
            : base(nameof(UnlockTokenProvider), log)
        {
            _registryRootKey = rootKey ?? throw new ArgumentNullException(nameof(rootKey));
        }

        public string GetUnlockToken()
        {
            var unlockToken = _registryRootKey.GetValue(REG_UNLOCK_TOKEN_NAME, string.Empty) as string;
            return unlockToken;
        }

        public void SaveUnlockToken(string token)
        {
            _registryRootKey.SetValue(REG_UNLOCK_TOKEN_NAME, token, RegistryValueKind.String);
        }
    }
}
