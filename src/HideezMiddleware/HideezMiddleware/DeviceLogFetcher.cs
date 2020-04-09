using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class DeviceLogFetcher : Logger
    {
        private class DailyDeviceLog
        {
            public DateTime Date { get; set; }
            public List<DeviceLogEntry> Entries { get; set; }
        }

        readonly ISettingsManager<ServiceSettings> _serviceSettingsManager;
        readonly ConnectionFlowProcessor _connectionFlowProcessor;

        readonly string _deviceLogsDirectoryPath;

        public DeviceLogFetcher(string deviceLogsDirectoryPath, ISettingsManager<ServiceSettings> serviceSettingsManager, ConnectionFlowProcessor connectionFlowProcessor, ILog log)
            : base(nameof(DeviceLogFetcher), log)
        {
            _deviceLogsDirectoryPath = deviceLogsDirectoryPath;
            _serviceSettingsManager = serviceSettingsManager;
            _connectionFlowProcessor = connectionFlowProcessor;

            _connectionFlowProcessor.DeviceFinishedMainFlow += ConnectionFlowProcessor_DeviceFinishedMainFlow;

            if (!Directory.Exists(deviceLogsDirectoryPath))
                Directory.CreateDirectory(deviceLogsDirectoryPath);
        }

        async void ConnectionFlowProcessor_DeviceFinishedMainFlow(object sender, IDevice device)
        {
            try
            {
                if (!_serviceSettingsManager.Settings.ReadDeviceLog)
                    return;

                var fullLog = await FetchDeviceLog(device);
                var dailyDeviceLogs = SplitByDays(fullLog);

                foreach (var dailyLog in dailyDeviceLogs)
                    SaveLog(dailyLog, device.SerialNo);

                if (_serviceSettingsManager.Settings.ClearDeviceLogsAfterRead)
                    await ClearDeviceLog(dailyDeviceLogs, device);
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        async Task<List<DeviceLogEntry>> FetchDeviceLog(IDevice device)
        {
            var rawLog = await device.FetchLog();

            if (rawLog.Length > 0)
                return DeviceLogParser.ParseLog(rawLog);
            else
                return new List<DeviceLogEntry>();
        }

        List<DailyDeviceLog> SplitByDays(List<DeviceLogEntry> logs)
        {
            if (logs.Count == 0)
                return new List<DailyDeviceLog>();

            var dailyLogsList = new List<DailyDeviceLog>();

            var firstLogWithTime = logs.FirstOrDefault(l => l.Time != null && l.Time != DateTime.MinValue);
            if (firstLogWithTime == null)
            {
                // All log entries lack time
                dailyLogsList.Add(new DailyDeviceLog() { Entries = logs });
            }
            else
            {
                var dailyLog = new DailyDeviceLog()
                {
                    Date = firstLogWithTime.Time.Date,
                    Entries = new List<DeviceLogEntry>(),
                };
                dailyLogsList.Add(dailyLog);

                for (int i = 0; i < logs.Count; i++)
                {
                    if (logs[i].Time == null || logs[i].Time.Date <= dailyLog.Date)
                    {
                        dailyLog.Entries.Add(logs[i]);
                        continue;
                    }

                    if (logs[i].Time.Date > dailyLog.Date)
                    {
                        dailyLog = new DailyDeviceLog()
                        {
                            Date = logs[i].Time.Date,
                            Entries = new List<DeviceLogEntry>() { logs[i] },
                        };
                        dailyLogsList.Add(dailyLog);
                        continue;
                    }
                }
            }

            return dailyLogsList;
        }

        string GetLogFileName(DailyDeviceLog deviceLog)
        {
            const string fileDateFormat = "dd-M-yyyy";
            const string fileExtension = ".txt";

            if (deviceLog.Date == null || deviceLog.Date == DateTime.MinValue)
                return $"unspecified time - {DateTime.Now.Date.ToString(fileDateFormat)}.{fileExtension}";
            else
                return $"{deviceLog.Date.ToString(fileDateFormat)}.{fileExtension}";
        }

        void CreateNewLog(DailyDeviceLog deviceLog, string logFilePath)
        {
            using (var sw = File.CreateText(logFilePath))
            {
                sw.WriteLine("======== Start of the log file ========");
                foreach (var entry in deviceLog.Entries)
                    sw.WriteLine(entry.ToString());
                sw.WriteLine("======== Log end ========");
            }
        }

        void AppendLog(DailyDeviceLog deviceLog, string logFilePath)
        {
            // Read already written data
            var dataInFile = new List<string>();
            using (var fs = File.Open(logFilePath, FileMode.Open))
            {

                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                        dataInFile.Add(sr.ReadLine());
                }
            }

            // Get the last entry with time
            var lastLineWithDate = dataInFile.LastOrDefault(l => !l.StartsWith(DeviceLogEntry.UNKNOWN_TIME_STR) && !l.StartsWith("="));

            using (var fs = File.Open(logFilePath, FileMode.Append))
            {
                using (var sw = new StreamWriter(fs))
                {
                    if (string.IsNullOrWhiteSpace(lastLineWithDate))
                    {
                        sw.WriteLine("======== No dated entries, reprinting full log ========");
                        foreach (var entry in deviceLog.Entries)
                            sw.WriteLine(entry.ToString());
                        sw.WriteLine("======== Log end ========");
                    }
                    else
                    {
                        var duplicateEntryIndex = deviceLog.Entries.FindIndex(e => e.ToString() == lastLineWithDate);
                        if (duplicateEntryIndex == -1)
                        {
                            sw.WriteLine("======== No matching dated entries, printing new log,  ========");
                            foreach (var entry in deviceLog.Entries)
                                sw.WriteLine(entry.ToString());
                            sw.WriteLine("======== Log end ========");
                        }
                        else
                        {
                            var filteredEntries = deviceLog.Entries.Skip(duplicateEntryIndex + 1); // Skip duplicated entry too
                            if (filteredEntries.Count() > 0)
                            {
                                sw.WriteLine("======== Found matching dated entry, continue printing since last dated entry ========");
                                foreach (var entry in filteredEntries)
                                    sw.WriteLine(entry.ToString());
                                sw.WriteLine("======== Log end ========");
                            }
                        }
                    }
                }
            }
        }

        void SaveLog(DailyDeviceLog deviceLog, string serialNo)
        {
            if (deviceLog.Entries.Count == 0)
                return;

            var path = Path.Combine(_deviceLogsDirectoryPath, serialNo);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var logFileName = GetLogFileName(deviceLog);

            var logFilePath = Path.Combine(path, logFileName);

            if (File.Exists(logFilePath))
                AppendLog(deviceLog, logFilePath);
            else
                CreateNewLog(deviceLog, logFilePath);
        }

        async Task ClearDeviceLog(List<DailyDeviceLog> dailyDeviceLogs, IDevice device)
        {
            // Check if device logs should be cleared
            if (_serviceSettingsManager.Settings.ClearDeviceLogsAfterDays == 0)
            {
                await device.ClearLog();
            }
            else if (dailyDeviceLogs.Count != 0)
            {
                var oldestLog = dailyDeviceLogs.OrderBy(l => l.Date).First();
                var deltaDays = (DateTime.Now - oldestLog.Date).TotalDays;
                if (_serviceSettingsManager.Settings.ClearDeviceLogsAfterDays < deltaDays)
                    await device.ClearLog();
            }
        }
    }
}
