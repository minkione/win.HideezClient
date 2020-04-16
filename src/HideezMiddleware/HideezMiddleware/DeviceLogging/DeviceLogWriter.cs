using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HideezMiddleware.DeviceLogging
{
    public class DeviceLogWriter : IDeviceLogWriter
    {
        public string SaveLog(string logsDirectoryPath, DailyDeviceLog dailyLog, IDevice device, bool includeDeviceMetadata)
        {
            if (dailyLog.Entries.Count == 0)
                return string.Empty;

            var specificDeviceLogsPath = Path.Combine(logsDirectoryPath, device.SerialNo);

            Directory.CreateDirectory(specificDeviceLogsPath);

            var logFileName = GetLogFileName(dailyLog, device.SerialNo);

            var logFilePath = Path.Combine(specificDeviceLogsPath, logFileName);

            if (File.Exists(logFilePath))
                AppendLog(dailyLog, logFilePath);
            else
                CreateNewLog(dailyLog, logFilePath, device, includeDeviceMetadata);

            return logFilePath;
        }

        string GetLogFileName(DailyDeviceLog deviceLog, string serialNo)
        {
            const string fileDateFormat = "yyyy-M-dd";
            const string fileExtension = ".txt";

            var serialNoLastDigits = serialNo.Substring(Math.Max(0, serialNo.Length - 5));

            var filenameSecondPart = $"hw {serialNoLastDigits}.{fileExtension}";

            if (deviceLog.Date == null || deviceLog.Date == DateTime.MinValue)
                return $"unspecified time - {DateTime.Now.Date.ToString(fileDateFormat)} {filenameSecondPart}";
            else
                return $"{deviceLog.Date.ToString(fileDateFormat)} {filenameSecondPart}";
        }

        void CreateNewLog(DailyDeviceLog deviceLog, string logFilePath, IDevice device, bool includeDeviceMetadata)
        {
            using (var sw = File.CreateText(logFilePath))
            {
                sw.WriteLine("======== Start of the log file ========");

                if (includeDeviceMetadata)
                {
                    // Write additional device metadata
                    sw.WriteLine($"=== Additional metadata ===");
                    sw.WriteLine($"== Collected at: {DateTime.Now}");
                    sw.WriteLine($"== Name: {device.Name}");
                    sw.WriteLine($"== Serial: {device.SerialNo}");
                    sw.WriteLine($"== Mac: {device.Mac}");
                    sw.WriteLine($"== Boot: {device.BootloaderVersion}");
                    sw.WriteLine($"== FW: {device.FirmwareVersion}");
                    sw.WriteLine($"== Battery: {device.Battery}");
                }

                // Write log into file
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
                        sw.WriteLine($"======== No dated entries, reprinting full log, {DateTime.Now} ========");
                        foreach (var entry in deviceLog.Entries)
                            sw.WriteLine(entry.ToString());
                        sw.WriteLine("======== Log end ========");
                    }
                    else
                    {
                        var duplicateEntryIndex = deviceLog.Entries.FindIndex(e => e.ToString() == lastLineWithDate);
                        if (duplicateEntryIndex == -1)
                        {
                            sw.WriteLine($"======== No matching dated entries, printing new log, {DateTime.Now}  ========");
                            foreach (var entry in deviceLog.Entries)
                                sw.WriteLine(entry.ToString());
                            sw.WriteLine("======== Log end ========");
                        }
                        else
                        {
                            var filteredEntries = deviceLog.Entries.Skip(duplicateEntryIndex + 1); // Skip duplicated entry too
                            if (filteredEntries.Count() > 0)
                            {
                                sw.WriteLine($"======== Found matching dated entry, continue printing since last dated entry, {DateTime.Now} ========");
                                foreach (var entry in filteredEntries)
                                    sw.WriteLine(entry.ToString());
                                sw.WriteLine("======== Log end ========");
                            }
                        }
                    }
                }
            }
        }
    }
}
