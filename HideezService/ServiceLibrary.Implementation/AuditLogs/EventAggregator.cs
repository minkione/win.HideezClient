using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.WorkstationEvents;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ServiceLibrary.Implementation
{
    class EventAggregator
    {
        private readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly HesAppConnection hesAppConnection;
        private readonly string eventDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\Hideez\WorkstationEvents\";
        private readonly List<WorkstationEvent> workstationEvents = new List<WorkstationEvent>();
        private readonly object lockObj = new object();
        private readonly System.Timers.Timer sendTimer = new System.Timers.Timer();
        private readonly double timeIntervalSend = 1_000;
        private readonly int minCountForSend = 10;
        // default is false, set 1 for true.
        private int _threadSafeBoolBackValue = 0;

        public EventAggregator(HesAppConnection hesAppConnection)
        {
            if (!Directory.Exists(eventDirectory))
            {
                Directory.CreateDirectory(eventDirectory);
            }

            this.hesAppConnection = hesAppConnection;
            sendTimer.Interval = timeIntervalSend;
            sendTimer.Elapsed += SendTimer_Elapsed;
            sendTimer.Start();
        }

        private bool IsSendToServer
        {
            get { return (Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 1) == 1); }
            set
            {
                if (value) Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 1, 0);
                else Interlocked.CompareExchange(ref _threadSafeBoolBackValue, 0, 1);
            }
        }

        private void SendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task task = SendEventsAsync();
        }

        public async Task AddNewAsync(WorkstationEvent workstationEvent, bool forceSendNow = false)
        {
            await Task.Run(async () =>
            {
                try
                {
                    log.Info("Added new workstation event.");
                    lock (lockObj)
                    {
                        workstationEvent.Version = WorkstationEvent.CurrentVersion;
                        workstationEvents.Add(workstationEvent);
                        string json = JsonConvert.SerializeObject(workstationEvent);
                        string file = $"{eventDirectory}{workstationEvent.Id}";
                        File.WriteAllText(file, json);
                        // File.SetAttributes(file, FileAttributes.ReadOnly | FileAttributes.Hidden);
                    }

                    if (forceSendNow || workstationEvents.Count >= minCountForSend)
                    {
                        await SendEventsAsync();
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    Debug.Assert(false);
                }
            });
        }

        private async Task SendEventsAsync()
        {
            try
            {
                if (hesAppConnection != null 
                    && hesAppConnection.State == HesConnectionState.Connected 
                    && !IsSendToServer 
                    && workstationEvents.Count > 0)
                {
                    log.Info("Sending to HES workstation events.");
                    IsSendToServer = true;
                    List<WorkstationEvent> newQueue = null;
                    sendTimer.Stop();
                    lock (lockObj)
                    {
                        newQueue = new List<WorkstationEvent>(workstationEvents);
                    }

                    log.Info($"Current event queue length: {newQueue.Count}");
                    await SendEventToServerAsync(newQueue);

                    newQueue.Clear();
                    try
                    {
                        lock (lockObj)
                        {
                            foreach (var file in Directory.GetFiles(eventDirectory))
                            {
                                try
                                {
                                    string data = File.ReadAllText(file);
                                    dynamic jsonObj = JsonConvert.DeserializeObject(data);
                                    string versionData = jsonObj[nameof(WorkstationEvent.Version)];

                                    if (versionData == WorkstationEvent.CurrentVersion)
                                    {
                                        WorkstationEvent we = JsonConvert.DeserializeObject<WorkstationEvent>(data);

                                        if (workstationEvents.FindIndex(e => e.Id == we.Id) < 0)
                                        {
                                            newQueue.Add(we);
                                        }
                                    }
                                    else
                                    {
                                        throw new NotSupportedException($"This version: {versionData} data for deserialize workstation event is not supported.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error(ex);
                                    Debug.Assert(false);
                                    try
                                    {
                                        // File.SetAttributes(file, FileAttributes.Normal);
                                        File.Delete(file);
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error(e);
                                    }
                                }
                            }
                        }

                        await SendEventToServerAsync(newQueue);

                        log.Info("End sending to HES workstation events.");
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                        Debug.Assert(false);
                    }
                }
            }
            finally
            {
                sendTimer.Start();
                IsSendToServer = false;
            }
        }

        public static IEnumerable<IEnumerable<T>> SplitIntoSets<T>(IEnumerable<T> source, int itemsPerSet)
        {
            var sourceList = source as List<T> ?? source.ToList();
            for (var index = 0; index < sourceList.Count; index += itemsPerSet)
            {
                yield return sourceList.Skip(index).Take(itemsPerSet);
            }
        }

        private async Task SendEventToServerAsync(List<WorkstationEvent> listEvents)
        {
            foreach (var set in SplitIntoSets(listEvents, 50))
            {
                if (set != null && set.Any()
                    && hesAppConnection != null
                    && hesAppConnection.State == HesConnectionState.Connected)
                {
                    // StringBuilder sb = new StringBuilder();
                    // foreach (var item in set)
                    // {
                    //    sb.Append(JsonConvert.SerializeObject(item));
                    // }
                    // var sizeOfMessage = System.Text.ASCIIEncoding.ASCII.GetByteCount(sb.ToString());

                    try
                    {
                        if (await hesAppConnection.SaveClientEventsAsync(set.ToArray()))
                        {
                            lock (lockObj)
                            {
                                foreach (var we in set)
                                {
                                    try
                                    {
                                        int index = workstationEvents.FindIndex(e => e.Id == we.Id);
                                        if (index >= 0)
                                        {
                                            workstationEvents.RemoveAt(index);
                                        }
                                        string file = $"{eventDirectory}{we.Id}";
                                        File.Delete(file);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error(ex);
                                        Debug.Assert(false);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                        Debug.Assert(false);
                    }
                }

            }
        }
    }
}
