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
        private readonly Timer sendTimer = new Timer();
        private readonly double timeIntervalSend = 1_000;
        private readonly int minCountForSend = 10;

        public EventAggregator(HesAppConnection hesAppConnection)
        {
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
                        workstationEvents.Add(workstationEvent);
                        string json = JsonConvert.SerializeObject(workstationEvent);
                        File.WriteAllText($"{eventDirectory}{workstationEvent.ID}", json);
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
                foreach (var file in Directory.GetFiles(eventDirectory))
                {
                    try
                    {
                        WorkstationEvent we = JsonConvert.DeserializeObject(File.ReadAllText(file)) as WorkstationEvent;
                        newQueue.Add(we);
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
            sendTimer.Start();
        }

        private async Task SendEventToServerAsync(List<WorkstationEvent> newQueue)
        {
            if (newQueue == null) return;

            foreach (var we in newQueue)
            {
                if (we == null) continue;

                try
                {
                    if (hesAppConnection != null && hesAppConnection.State == HesConnectionState.Connected)
                    {
                        if (await hesAppConnection.SendWorkstationEventAsync(we))
                        {
                            workstationEvents.Remove(we);
                            File.Delete($"{eventDirectory}{we.ID}");
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
