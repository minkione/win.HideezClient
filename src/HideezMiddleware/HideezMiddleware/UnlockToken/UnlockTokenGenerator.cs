using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using HideezMiddleware.Settings;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;

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

        readonly string _CPImagePath = Path.Combine(Environment.SystemDirectory, "HideezCredentialProvider3.bmp");

        readonly object _isRunningLock = new object();
        bool _isRunning;

        public UnlockTokenGenerator(IUnlockTokenProvider unlockTokenProvider, IWorkstationInfoProvider workstationInfoProvider, ILog log)
            : base(nameof(UnlockTokenGenerator), log)
        {
            _unlockTokenProvider = unlockTokenProvider ?? throw new ArgumentNullException(nameof(unlockTokenProvider));
            _workstationInfoProvider = workstationInfoProvider ?? throw new ArgumentNullException(nameof(workstationInfoProvider));
            _qrUnlockTokenFactory = new UnlockQrFactory(_workstationInfoProvider, log);

            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;
        }

        public void Start()
        {
            lock (_isRunningLock)
            {
                if (!_isRunning)
                {
                    if (!WorkstationHelper.IsActiveSessionLocked())
                        GenerateAndSaveUnlockToken();

                    // Ensure that during the first launch on locked workstation unlock token is still generated
                    if (_unlockTokenProvider.GetUnlockToken() == string.Empty)
                        GenerateAndSaveUnlockToken();

                    _isRunning = true;
                    WriteLine("Started");
                }
            }
        }

        public void Stop()
        {
            lock (_isRunningLock)
            {
                if (_isRunning)
                {
                    _isRunning = false;
                    WriteLine("Stopped");
                }
            }
        }

        public void DeleteSavedToken()
        {
            // We only cake about the token in form of QR
            File.Delete(_CPImagePath);
        }

        void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            if (_isRunning)
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
                using (var fs = new FileStream(_CPImagePath, FileMode.Create))
                {
                    qrBitmap.Save(fs, ImageFormat.Bmp);
                }
            }
        }
    }
}
