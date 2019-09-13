using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.WorkstationEvents;
using Newtonsoft.Json;

namespace ServiceLibrary.Implementation
{
    class EventAggregator : Logger
    {
        const double FORCE_SEND_INTERVAL = 300_000;
        const int MIN_EVENTS_FOR_SEND = 20;
        const int EVENTS_PER_SET = 25;
        const int SET_INTERVAL = 5_000; // Interval between multiple sets

        readonly HesAppConnection _hesAppConnection;
        readonly FileSystemWatcher _fileSystemWatcher;

        readonly string eventDirectoryPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Hideez\WorkstationEvents\";
        readonly System.Timers.Timer automaticEventSendingTimer = new System.Timers.Timer(FORCE_SEND_INTERVAL);

        // default is false, set 1 for true.
        int _threadSafeBoolBackValue = 0;

        public EventAggregator(HesAppConnection hesAppConnection, ILog log)
            : base(nameof(EventAggregator), log)
        {
            if (!Directory.Exists(eventDirectoryPath))
                Directory.CreateDirectory(eventDirectoryPath);

            _hesAppConnection = hesAppConnection;
            _fileSystemWatcher = new FileSystemWatcher(eventDirectoryPath);
            _fileSystemWatcher.Created += FileSystemWatcher_OnFileCreated;

            automaticEventSendingTimer.Elapsed += SendTimer_Elapsed;
            automaticEventSendingTimer.Start();
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
                if (Directory.Exists(eventDirectoryPath))
                    return Directory.GetFiles(eventDirectoryPath).Count();
            }
            catch (Exception) { }

            return 0;
        }

        public async Task AddNewAsync(WorkstationEvent workstationEvent, bool forceSendNow = false)
        {
            await Task.Run(async () =>
            {
                try
                {
                    WriteLine($"New workstation event: {workstationEvent.EventId}");

                    string json = JsonConvert.SerializeObject(workstationEvent);
                    string file = $"{eventDirectoryPath}{workstationEvent.Id}";
                    File.WriteAllText(file, json);

                    if (forceSendNow)
                        await SendEventsAsync(true);
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
            });
        }

        IEnumerable<IEnumerable<T>> SplitIntoSets<T>(IEnumerable<T> source, int itemsPerSet)
        {
            var sourceList = source as List<T> ?? source.ToList();
            for (var index = 0; index < sourceList.Count; index += itemsPerSet)
            {
                yield return sourceList.Skip(index).Take(itemsPerSet);
            }
        }

        List<WorkstationEvent> DeserializeEvents()
        {
            List<WorkstationEvent> events = new List<WorkstationEvent>();

            if (!Directory.Exists(eventDirectoryPath))
                return events;

            try
            {
                var eventFiles = Directory.GetFiles(eventDirectoryPath);
                
                foreach (var file in eventFiles)
                {
                    try
                    {
                        string data = File.ReadAllText(file);
                        dynamic jsonObj = JsonConvert.DeserializeObject(data);
                        string eventVersion = jsonObj[nameof(WorkstationEvent.Version)];

                        if (eventVersion != WorkstationEvent.CurrentVersion)
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

        async Task SendEventsAsync(bool skipSetInterval = false)
        {
            if (_hesAppConnection != null 
                && _hesAppConnection.State == HesConnectionState.Connected 
                && GetEventsCount() > 0 
                && Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 0) == 0)
            {
                try
                { 
                    WriteLine("Sending workstation events to HES");
                    automaticEventSendingTimer.Stop();

                    var eventsQueue = DeserializeEvents();
                    WriteLine($"Current event queue length: {eventsQueue.Count}");

                    await BreakIntoSetsAndSendToServer(eventsQueue, skipSetInterval);

                    WriteLine("End sending to HES workstation events.");
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                }
                finally
                {
                    Interlocked.Exchange(ref _threadSafeBoolBackValue, 0);
                    automaticEventSendingTimer.Start();
                }
            }
        }

        async Task BreakIntoSetsAndSendToServer(List<WorkstationEvent> listEvents, bool skipSendDelay = false)
        {
            foreach (var set in SplitIntoSets(listEvents, EVENTS_PER_SET))
            {
                if (set != null && set.Any()
                    && _hesAppConnection != null
                    && _hesAppConnection.State == HesConnectionState.Connected)
                {
                    try
                    {
                        if (await _hesAppConnection.SaveClientEventsAsync(set.ToArray()))
                        {
                            foreach (var we in set)
                            {
                                try
                                {
                                    var file = Path.Combine(eventDirectoryPath, we.Id);
                                    File.Delete(file);
                                }
                                catch (Exception ex)
                                {
                                    WriteLine(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLine(ex);
                    }

                    // Todo: (EventAggregator) Set delay should not be called if current set is the last set
                    if (!skipSendDelay)
                        await Task.Delay(SET_INTERVAL);
                }

            }
        }
    }
}
