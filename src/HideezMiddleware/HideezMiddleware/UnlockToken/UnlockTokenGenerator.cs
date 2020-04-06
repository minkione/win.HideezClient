using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Microsoft.Win32;
using System;

namespace HideezMiddleware.UnlockToken
{
    /// <summary>
    /// Ensures that new unlock token is generated when user is logged into session or service is restarted in unlocked session
    /// </summary>
    public class UnlockTokenGenerator : Logger
    {
        readonly IUnlockTokenProvider _unlockTokenProvider;

        public UnlockTokenGenerator(IUnlockTokenProvider unlockTokenProvider, ILog log)
            : base(nameof(UnlockTokenGenerator), log)
        {
            _unlockTokenProvider = unlockTokenProvider ?? throw new ArgumentNullException(nameof(unlockTokenProvider));

            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;

            if (!WorkstationHelper.IsActiveSessionLocked())
                GenerateAndSaveUnlockToken();
            
            // Ensure that during the first launch on locked workstation unlock token is still generated
            if (unlockTokenProvider.GetUnlockToken() == string.Empty)
                GenerateAndSaveUnlockToken();
        }

        void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            switch (reason)
            {
                case SessionSwitchReason.SessionLogon:
                case SessionSwitchReason.SessionUnlock:
                    GenerateAndSaveUnlockToken();
                    break;
                default:
                    break;
            }
        }

        string GenerateUnlockToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        void GenerateAndSaveUnlockToken()
        {
            var newToken = GenerateUnlockToken();
            SaveUnlockTokenToRegistry(newToken);
            SaveUnlockTokenToSystem32(newToken);
        }

        void SaveUnlockTokenToRegistry(string token)
        {
            _unlockTokenProvider.SaveUnlockToken(token);
        }

        void SaveUnlockTokenToSystem32(string token)
        {
            // TODO:
        }
    }
}
