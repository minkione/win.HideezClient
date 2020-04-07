using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using Microsoft.Win32;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using ZXing;
using ZXing.QrCode;

namespace HideezMiddleware.UnlockToken
{
    /// <summary>
    /// Ensures that new unlock token is generated when user is logged into session or service is restarted in unlocked session
    /// </summary>
    public class UnlockTokenGenerator : Logger
    {
        readonly IUnlockTokenProvider _unlockTokenProvider;
        readonly IWorkstationInfoProvider _workstationInfoProvider;
        readonly UnlockQrFactory _qrUnlockTokenFactory;

        readonly string CPImagePath = Path.Combine(Environment.SystemDirectory, "HideezCredentialProvider3.bmp");

        public UnlockTokenGenerator(IUnlockTokenProvider unlockTokenProvider, IWorkstationInfoProvider workstationInfoProvider, ILog log)
            : base(nameof(UnlockTokenGenerator), log)
        {
            _unlockTokenProvider = unlockTokenProvider ?? throw new ArgumentNullException(nameof(unlockTokenProvider));
            _workstationInfoProvider = workstationInfoProvider ?? throw new ArgumentNullException(nameof(workstationInfoProvider));
            _qrUnlockTokenFactory = new UnlockQrFactory(_workstationInfoProvider, log);

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
            for (int tryCount = 0; tryCount < 2; tryCount++)
            {
                try
                {
                    WriteLine("Generating new unlock token");
                    var newToken = GenerateUnlockToken();
                    SaveUnlockTokenToRegistry(newToken);
                    SaveUnlockTokenToSystem32(newToken);
                    WriteLine($"New token generated and saved: {newToken}");

                    break;
                }
                catch (Exception ex)
                {
                    WriteLine(ex);

                    if (tryCount < 2)
                        WriteLine("Retry once...");
                    else
                        break;
                }
            }
        }

        void SaveUnlockTokenToRegistry(string token)
        {
            WriteLine("Saving token in registry");
            _unlockTokenProvider.SaveUnlockToken(token);
        }

        void SaveUnlockTokenToSystem32(string token)
        {
            WriteLine("Saving token in system");
            using (var qrBitmap = _qrUnlockTokenFactory.GenerateNewUnlockQr(token))
            {
                using (var fs = new FileStream(CPImagePath, FileMode.Create))
                {
                    qrBitmap.Save(fs, ImageFormat.Bmp);
                }
            }
        }
    }
}
