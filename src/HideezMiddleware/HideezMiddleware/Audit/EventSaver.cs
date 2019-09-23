using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.WorkstationEvents;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HideezMiddleware.Audit
{
    public class EventSaver : Logger
    {
        readonly EventFactory _eventFactory;

        public event EventHandler UrgentEventSaved;

        public EventSaver(string eventsDirectoryPath, ILog log)
            : base(nameof(EventSaver), log)
        {
            EventsDirectoryPath = eventsDirectoryPath;

            if (!Directory.Exists(EventsDirectoryPath))
                Directory.CreateDirectory(EventsDirectoryPath);

            _eventFactory = new EventFactory(log);
        }

        public string EventsDirectoryPath { get; }

        public WorkstationEvent GetWorkstationEvent()
        {
            return _eventFactory.GetWorkstationEvent();
        }

        public WorkstationEvent GetPrevSessionWorkstationEvent()
        {
            return _eventFactory.GetWorkstationEvent();
        }

        /// <summary>
        /// Add new workstation event to the events queue
        /// </summary>
        /// <param name="workstationEvent">Workstation event to queue</param>
        /// <param name="sendImmediatelly">If true, event sending is performed immediatelly after queuing event</param>
        public async Task AddNewAsync(WorkstationEvent workstationEvent, bool sendImmediatelly = false)
        {
            await Task.Run(() =>
            {
                try
                {
                    WriteLine($"New workstation event: {workstationEvent.EventId}");

                    string json = JsonConvert.SerializeObject(workstationEvent);
                    string file = $"{EventsDirectoryPath}{workstationEvent.Id}";
                    File.WriteAllText(file, json);

                    if (sendImmediatelly)
                        UrgentEventSaved?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
            });
        }
    }
}
