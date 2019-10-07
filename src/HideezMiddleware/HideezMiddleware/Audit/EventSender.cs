using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.WorkstationEvents;
using Newtonsoft.Json;

namespace HideezMiddleware.Audit
{
    public class EventSender : Logger
    {
        // Once per 15 minutes seems ok for release.
        // It equals to 36 checks per work day
#if DEBUG
        const double AUTO_SEND_INTERVAL = 30_000;
#else
        const double AUTO_SEND_INTERVAL = 900_000;
# endif

        const int MIN_EVENTS_FOR_SEND = 20;
        const int EVENTS_PER_SET = 25;
        const int SET_INTERVAL = 5_000; // Interval between multiple sets

        readonly HesAppConnection _hesAppConnection;
        readonly FileSystemWatcher _fileSystemWatcher;
        readonly EventSaver _eventSaver;

        readonly System.Timers.Timer automaticEventSendingTimer = new System.Timers.Timer(AUTO_SEND_INTERVAL);

        // Used for interlocking event sending methods
        int _sendingThreadSafetyInt = 0;

        // Used for locking operations with events
        readonly object _fileSystemLock = new object();

        public EventSender(HesAppConnection hesAppConnection, EventSaver eventSaver, ILog log)
            : base(nameof(EventSender), log)
        {
            _eventSaver = eventSaver;
            _eventSaver.UrgentEventSaved += EventSaver_UrgentEventSaved;

            _hesAppConnection = hesAppConnection;
            _fileSystemWatcher = new FileSystemWatcher(_eventSaver.EventsDirectoryPath);
            _fileSystemWatcher.Created += FileSystemWatcher_OnFileCreated;
            _fileSystemWatcher.EnableRaisingEvents = true;

            automaticEventSendingTimer.Elapsed += SendTimer_Elapsed;
            automaticEventSendingTimer.Start();
            SendTimer_Elapsed(null, null); // Trigger 15 minute events check
        }

        async void EventSaver_UrgentEventSaved(object sender, EventArgs e)
        {
            try
            {
                await SendEventsAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        async void FileSystemWatcher_OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (GetEventsCount() >= MIN_EVENTS_FOR_SEND)
                {
                    await SendEventsAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        async void SendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await SendEventsAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        int GetEventsCount()
        {
            try
            {
                if (Directory.Exists(_eventSaver.EventsDirectoryPath))
                    return Directory.GetFiles(_eventSaver.EventsDirectoryPath).Count();
            }
            catch (Exception) { }

            return 0;
        }

        /// <summary>
        /// Deserializes all events saved in events folder and returns them as List<>
        /// </summary>
        /// <returns>Returns all events deserialized from events folder</returns>
        List<WorkstationEvent> DeserializeEvents()
        {
            List<WorkstationEvent> events = new List<WorkstationEvent>();

            if (!Directory.Exists(_eventSaver.EventsDirectoryPath))
                return events;

            try
            {
                var eventFiles = Directory.GetFiles(_eventSaver.EventsDirectoryPath).OrderBy(f => new FileInfo(f).LastWriteTimeUtc);

                foreach (var file in eventFiles)
                {
                    try
                    {
                        string data = File.ReadAllText(file);
                        dynamic jsonObj = JsonConvert.DeserializeObject(data);
                        string eventVersion = jsonObj[nameof(WorkstationEvent.Version)];

                        if (eventVersion != WorkstationEvent.ClassVersion)
                            throw new NotSupportedException($"This version: {eventVersion} data for deserialize workstation event is not supported.");

                        WorkstationEvent we = JsonConvert.DeserializeObject<WorkstationEvent>(data);
                        events.Add(we);
                    }
                    catch (Exception ex)
                    {
                        WriteLine(ex);

                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception e)
                        {
                            WriteLine(e);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }

            return events;
        }

        /// <summary>
        /// Delete specified event files in the event folder
        /// </summary>
        /// <param name="workstationEvents"></param>
        void DeleteEvents(IEnumerable<WorkstationEvent> workstationEvents)
        {
            foreach (var we in workstationEvents)
            {
                try
                {
                    var file = Path.Combine(_eventSaver.EventsDirectoryPath, we.Id);
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
            }
        }

        async Task SendEventsAsync(bool skipSetInterval = false)
        {
            if (_hesAppConnection != null
                && _hesAppConnection.State == HesConnectionState.Connected
                && Interlocked.CompareExchange(ref _sendingThreadSafetyInt, 1, 0) == 0)
            {
                try
                {
                    if (GetEventsCount() == 0)
                        return;

                    automaticEventSendingTimer.Stop();

                    var eventsQueue = DeserializeEvents();
                    WriteLine($"Sending {eventsQueue.Count} " + (eventsQueue.Count == 1 ? "event" : "events") + " to HES");

                    await BreakIntoSetsAndSendToServer(eventsQueue, skipSetInterval);
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
                finally
                {
                    Interlocked.Exchange(ref _sendingThreadSafetyInt, 0);
                    automaticEventSendingTimer.Start();
                }
            }
        }

        async Task BreakIntoSetsAndSendToServer(IEnumerable<WorkstationEvent> events, bool skipSendDelay = false)
        {
            var sets = events.ToSets(EVENTS_PER_SET).ToList();

            if (sets.Count > 1)
                WriteLine($"Split events into {sets.Count} sets of {EVENTS_PER_SET} items");

            for (int i = 0; i < sets.Count; i++)
            {
                if (_hesAppConnection?.State == HesConnectionState.Connected && sets[i] != null && sets[i].Any())
                {
                    try
                    {
                        var IsServerProcessedEvents = await _hesAppConnection.SaveClientEventsAsync(sets[i].ToArray());

                        if (IsServerProcessedEvents)
                        {
                            WriteLine($"Sent events set. Server: ok");
                            DeleteEvents(sets[i]);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLine(ex);
                    }

                    // Add delay between multiple sendings of sets
                    // Skip delay if sent last set
                    if ((i < sets.Count - 1) && !skipSendDelay)
                        await Task.Delay(SET_INTERVAL);
                }
            }
        }
    }
}
