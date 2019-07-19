using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
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

        public async Task AddNewAsync(WorkstationEvent workstationEvent)
        {
            await Task.Run(async () =>
            {
                try
                {
                    lock (lockObj)
                    {
                        workstationEvent.Version = WorkstationEvent.CurrentVersion;
                        workstationEvents.Add(workstationEvent);
                        string json = JsonConvert.SerializeObject(workstationEvent);
                        File.WriteAllText($"{eventDirectory}{workstationEvent.Id}", json);
                    }

                    if (workstationEvents.Count >= minCountForSend)
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
                if (hesAppConnection != null && hesAppConnection.State == HesConnectionState.Connected && !IsSendToServer)
                {
                    IsSendToServer = true;
                    List<WorkstationEvent> newQueue = null;
                    sendTimer.Stop();
                    lock (lockObj)
                    {
                        newQueue = new List<WorkstationEvent>(workstationEvents);
                    }

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
                                        File.Delete(file);
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error(ex);
                                    }
                                }
                            }
                        }

                        await SendEventToServerAsync(newQueue);

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

        private async Task SendEventToServerAsync(List<WorkstationEvent> newQueue)
        {
            if (newQueue != null && newQueue.Any()
                && hesAppConnection != null
                && hesAppConnection.State == HesConnectionState.Connected)
            {
                try
                {
                    if (await hesAppConnection.SaveClientEventsAsync(newQueue.ToArray()))
                    {
                        lock (lockObj)
                        {
                            foreach (var we in newQueue)
                            {
                                try
                                {
                                    int index = workstationEvents.FindIndex(e => e.Id == we.Id);
                                    if (index >= 0)
                                    {
                                        workstationEvents.RemoveAt(index);
                                    }
                                    File.Delete($"{eventDirectory}{we.Id}");
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
