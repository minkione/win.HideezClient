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
                if (hesAppConnection != null && hesAppConnection.State == HesConnectionState.Connected)
                {
                    List<WorkstationEvent> newQueue = null;
                    sendTimer.Stop();
                    Monitor.Enter(lockObj);

                    newQueue = new List<WorkstationEvent>(workstationEvents);
                    await SendEventToServerAsync(newQueue);

                    newQueue.Clear();
                    try
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
                                    newQueue.Add(we);
                                }
                                else
                                {
                                    log.Error($"This version: {versionData} data for deserialize workstation event is not supported.");
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                                Debug.Assert(false);
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
                Monitor.Exit(lockObj);
                sendTimer.Start();
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
                        foreach (var we in newQueue)
                        {
                            try
                            {
                                workstationEvents.Remove(we);
                                File.Delete($"{eventDirectory}{we.Id}");
                                Debug.WriteLine($"################ File delited {we.Id}");
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                                Debug.Assert(false);
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
