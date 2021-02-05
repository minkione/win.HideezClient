using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using Hideez.SDK.Communication.WorkstationEvents;
using HideezMiddleware.Workstation;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HideezMiddleware.Audit
{
    public class EventSaver : Logger
    {
        readonly EventFactory _eventFactory;
        readonly object _writeLock = new object();

        public event EventHandler UrgentEventSaved;

        public EventSaver(ISessionInfoProvider sessionInfoProvider, IWorkstationIdProvider workstationIdProvider, string eventsDirectoryPath, ILog log)
            : base(nameof(EventSaver), log)
        {
            EventsDirectoryPath = eventsDirectoryPath;

            if (!Directory.Exists(EventsDirectoryPath))
                Directory.CreateDirectory(EventsDirectoryPath);

            _eventFactory = new EventFactory(sessionInfoProvider, workstationIdProvider, log);
        }


        public string EventsDirectoryPath { get; }

        public WorkstationEvent GetWorkstationEvent()
        {
            return _eventFactory.GetWorkstationEvent();
        }

        public WorkstationEvent GetPrevSessionWorkstationEvent()
        {
            return _eventFactory.GetPreviousSessionEvent();
        }

        /// <summary>
        /// Add new workstation event to the events queue
        /// </summary>
        /// <param name="workstationEvent">Workstation event to queue</param>
        /// <param name="sendImmediatelly">If true, event sending is performed immediatelly after queuing event</param>
        public async Task AddNewAsync(WorkstationEvent workstationEvent, bool sendImmediatelly = false)
        {
            await Task.Run(() => { AddNew(workstationEvent, sendImmediatelly); });
        }

        public void AddNew(WorkstationEvent workstationEvent, bool sendImmediatelly = false)
        {
            lock (_writeLock)
            {
                try
                {
                    WriteLine($"New event: {workstationEvent.EventId} - (session id: {workstationEvent.WorkstationSessionId})");

                    string json = JsonConvert.SerializeObject(workstationEvent);
                    string file = $"{EventsDirectoryPath}{workstationEvent.Id}";
                    File.WriteAllText(file, json);
                    File.SetCreationTimeUtc(file, workstationEvent.Date); // Event file creation date should match the event time

                    if (sendImmediatelly)
                        UrgentEventSaved?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
            }
        }
    }
}
