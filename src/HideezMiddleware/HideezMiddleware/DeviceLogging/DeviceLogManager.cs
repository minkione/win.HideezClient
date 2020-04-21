using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceLogging
{
    public class DeviceLogManager : Logger
    {
        readonly string _logsDirectoryPath;

        readonly ISettingsManager<ServiceSettings> _serviceSettingsManager;
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly IDeviceLogWriter _deviceLogWriter;

        public DeviceLogManager(string logsDirectoryPath, IDeviceLogWriter deviceLogWriter, ISettingsManager<ServiceSettings> serviceSettingsManager, ConnectionFlowProcessor connectionFlowProcessor, ILog log)
            : base(nameof(DeviceLogManager), log)
        {
            _logsDirectoryPath = logsDirectoryPath;

            _deviceLogWriter = deviceLogWriter;
            _serviceSettingsManager = serviceSettingsManager;
            _connectionFlowProcessor = connectionFlowProcessor;

            _connectionFlowProcessor.DeviceFinishedMainFlow += ConnectionFlowProcessor_DeviceFinishedMainFlow;
        }

        async void ConnectionFlowProcessor_DeviceFinishedMainFlow(object sender, IDevice device)
        {
            try
            {
                if (!_serviceSettingsManager.Settings.ReadDeviceLog)
                    return;

                var deviceLogs = await FetchDeviceLog(device);

                foreach (var dailyLog in deviceLogs)
                    _deviceLogWriter.SaveLog(_logsDirectoryPath, dailyLog, device, true);

                if (ShouldClearInternalLogCheck(deviceLogs))
                    await device.ClearLog();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        async Task<List<DailyDeviceLog>> FetchDeviceLog(IDevice device)
        {
            var rawLog = await device.FetchLog();
            if (rawLog == null || rawLog.Length == 0)
                return new List<DailyDeviceLog>();
            else
                return DeviceLogParser.SplitByDays(DeviceLogParser.ParseLog(rawLog));
        }

        bool ShouldClearInternalLogCheck(List<DailyDeviceLog> dailyDeviceLogs)
        {
            if (_serviceSettingsManager.Settings.ClearDeviceLogsAfterRead)
            {
                // Always clear if delay in days set to 0
                if (_serviceSettingsManager.Settings.ClearDeviceLogsAfterDays == 0)
                    return true;

                if (dailyDeviceLogs.Count != 0)
                {
                    var oldestLog = dailyDeviceLogs.OrderBy(l => l.Date).First();
                    var deltaDays = (DateTime.Now - oldestLog.Date).TotalDays;

                    // Clear if oldest log was made more than <ClearDeviceLogsAfterDays> days ago
                    if (_serviceSettingsManager.Settings.ClearDeviceLogsAfterDays < deltaDays)
                        return true;
                }
            }
            
            return false;
        }
    }
}
