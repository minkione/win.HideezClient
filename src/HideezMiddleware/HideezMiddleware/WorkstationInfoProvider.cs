using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using HideezMiddleware.Settings;
using HideezMiddleware.Workstation;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;

namespace HideezMiddleware
{
    public class WorkstationInfoProvider : Logger, IWorkstationInfoProvider
    {
        readonly ISettingsManager<ServiceSettings> _serviceSettingsManager;
        readonly IWorkstationIdProvider _workstationIdProvider;

        public WorkstationInfoProvider(IWorkstationIdProvider workstationIdProvider, ILog log)
            : base(nameof(WorkstationInfoProvider), log)
        {
            _workstationIdProvider = workstationIdProvider;
        }

        public WorkstationInfoProvider(IWorkstationIdProvider workstationIdProvider, ISettingsManager<ServiceSettings> serviceSettingsManager, ILog log)
            : base(nameof(WorkstationInfoProvider), log)
        {
            _workstationIdProvider = workstationIdProvider;
            _serviceSettingsManager = serviceSettingsManager;
        }

        public string WorkstationId
        {
            get
            {
                return _workstationIdProvider.GetWorkstationId();
            }
        }

        public bool IsAlarmTurnOn
        {
            get
            {
                if (_serviceSettingsManager != null)
                    return _serviceSettingsManager.Settings.AlarmTurnOn;
                else return false;
            }
        }

        public WorkstationInfo GetWorkstationInfo()
        {
            WorkstationInfo workstationInfo = new WorkstationInfo();

            try
            {
                workstationInfo.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                workstationInfo.Id = WorkstationId;
                workstationInfo.MachineName = Environment.MachineName;
                workstationInfo.Domain = Environment.UserDomainName;

                try
                {
                    RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
                    workstationInfo.OsName = (string)registryKey.GetValue("ProductName");
                    workstationInfo.OsVersion = (string)registryKey.GetValue("ReleaseId");
                    workstationInfo.OsBuild = $"{registryKey.GetValue("CurrentBuild")}.{registryKey.GetValue("UBR")}";
                }
                catch (Exception ex)
                {
                    WriteLine(ex);
                    Debug.Assert(false, "An exception occured while querrying workstation operating system");
                }

                workstationInfo.Users = WorkstationHelper.GetAllUserNames();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
                Debug.Assert(false);
            }

            return workstationInfo;
        }
    }
}
