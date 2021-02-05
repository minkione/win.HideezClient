using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using HideezMiddleware.Utils.WorkstationHelper;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace HideezMiddleware.Audit
{
    /// <summary>
    /// Saves timestamp of the latest active session and generates a WorkstationLoock event if a record is found on startup
    /// </summary>
    public class SessionTimestampLogger : Logger
    {
        [Serializable]
        class SessionTimestamp
        {
            public DateTime Time { get; set; }
            public string SessionId { get; set; }
            public string SessionName { get; set; }
        }

        const double TIMESTAMP_SAVE_INTERVAL = 300_000; // 5 minutes. At most results in 5 minute difference between service uptime and shutdown without server connection

        readonly string _timestampFilePath;
        readonly ISessionInfoProvider _sessionInfoProvider;
        readonly EventSaver _eventSaver;
        readonly Timer _timestampSaveTimer = new Timer(TIMESTAMP_SAVE_INTERVAL);
        readonly IWorkstationHelper _workstationHelper;

        readonly object _fileLock = new object();

        public SessionTimestampLogger(string timestampFilePath, ISessionInfoProvider sessionInfoProvider, EventSaver eventSaver,
            IWorkstationHelper workstationHelper, ILog log)
            : base(nameof(SessionTimestampLogger), log)
        {
            _timestampFilePath = timestampFilePath;
            _sessionInfoProvider = sessionInfoProvider;
            _eventSaver = eventSaver;
            _workstationHelper = workstationHelper;

            _timestampSaveTimer.Elapsed += TimestampSaveTimer_Elapsed;

            var savedTimestamp = GetSavedTimestamp();
            if (savedTimestamp != null)
            {
                GenerateSessionEndEvent(savedTimestamp);
                ClearSavedTimestamp();
            }

            SessionSwitchMonitor.SessionSwitch += SessionSwitchMonitor_SessionSwitch;

            var state = _workstationHelper.GetSessionLockState(_workstationHelper.GetSessionId());
            if (state == WorkstationInformationHelper.LockState.Unlocked)
            {
                SaveOrUpdateTimestamp(CreateNewTimestamp());
                _timestampSaveTimer.Start();
            }
        }

        void TimestampSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SaveOrUpdateTimestamp(CreateNewTimestamp());
        }

        void SessionSwitchMonitor_SessionSwitch(int sessionId, SessionSwitchReason reason)
        {
            switch (reason)
            {
                case SessionSwitchReason.SessionLock:
                case SessionSwitchReason.SessionLogoff:
                    ClearSavedTimestamp();
                    _timestampSaveTimer.Stop();
                    break;
                case SessionSwitchReason.SessionUnlock:
                case SessionSwitchReason.SessionLogon:
                    _timestampSaveTimer.Stop();
                    var ts = CreateNewTimestamp();
                    // A workaround for race condition and incorrect order with unlock events on fast shutdown
                    ts.Time = ts.Time + TimeSpan.FromSeconds(30);  
                    SaveOrUpdateTimestamp(ts);
                    _timestampSaveTimer.Start();
                    break;
                default:
                    return;
            }
        }

        SessionTimestamp CreateNewTimestamp()
        {
            return new SessionTimestamp()
            {
                SessionId = _sessionInfoProvider.CurrentSession?.SessionId,
                SessionName = _sessionInfoProvider.CurrentSession?.SessionName,
                Time = DateTime.UtcNow,
            };
        }

        /// <summary>
        /// Get saved timestamp from the file
        /// </summary>
        /// <returns>Returns timestamp from timestamp file if present. Else returns null if file is empty or missing.</returns>
        SessionTimestamp GetSavedTimestamp()
        {
            lock (_fileLock)
            {
                if (!File.Exists(_timestampFilePath))
                    return null;

                try
                {
                    using (StreamReader sr = new StreamReader(_timestampFilePath))
                    {
                        var fileData = sr.ReadToEnd();
                        var savedTimestamp = JsonConvert.DeserializeObject<SessionTimestamp>(fileData);
                        return savedTimestamp;
                    }
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// Save specified timestamp into file
        /// </summary>
        void SaveOrUpdateTimestamp(SessionTimestamp timestamp)
        {
            lock (_fileLock)
            {
                try
                {
                    var dir = Path.GetDirectoryName(_timestampFilePath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    using (StreamWriter sw = new StreamWriter(_timestampFilePath, false))
                    {
                        var jsonData = JsonConvert.SerializeObject(timestamp);
                        sw.WriteLine(jsonData);
                    }
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Clear timestamp file
        /// </summary>
        void ClearSavedTimestamp()
        {
            lock (_fileLock)
            {
                File.Delete(_timestampFilePath);
            }
        }

        void GenerateSessionEndEvent(SessionTimestamp timestamp)
        {
            Task.Run(async () =>
            {
                var baseEvent = _eventSaver.GetWorkstationEvent();

                baseEvent.WorkstationSessionId = timestamp.SessionId;
                baseEvent.UserSession = timestamp.SessionName;
                baseEvent.Date = timestamp.Time;
                baseEvent.Note = "Unexpected Shutdown";
                baseEvent.EventId = WorkstationEventType.ComputerLock; // TODO: Maybe, add another event for Unexpected Shutdown

                await _eventSaver.AddNewAsync(baseEvent, true);
            });
        }
    }
}
