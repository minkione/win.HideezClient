using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Workstation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class WorkstationInfoProvider : IWorkstationInfoProvider
    {
        public ILog log;
        private readonly IPAddress allowedIP;

        public WorkstationInfoProvider(string allowedIP, ILog log)
        {
            this.log = log;
            IPAddress.TryParse(allowedIP, out IPAddress address);
            this.allowedIP = address;
        }

        public async Task<WorkstationInfo> GetWorkstationInfoAsync()
        {
#if DEBUG
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            WorkstationInfo workstationInfo = new WorkstationInfo();

            try
            {
                workstationInfo.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

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
                    log?.WriteLine(nameof(WorkstationInfoProvider), ex);
                    Debug.Assert(false);
                }

                if (allowedIP != null && allowedIP != IPAddress.None)
                {
                    IPAddress localIP = await WorkstationHelper.GetLocalIPAddressAsync(allowedIP);
                    PhysicalAddress mac = WorkstationHelper.GetCurrentMAC(localIP);

                    workstationInfo.IP = localIP.ToString();
                    workstationInfo.MAC = mac.ToString();
                }
                else
                {
                    log?.WriteLine(nameof(WorkstationInfoProvider), $"{nameof(allowedIP)} is null or none.", LogErrorSeverity.Error);
                }

                workstationInfo.Users = await WorkstationHelper.GetAllUserNamesAsync();
            }
            catch (Exception ex)
            {
                log?.WriteLine(nameof(WorkstationInfoProvider), ex);
                Debug.Assert(false);
            }

#if DEBUG
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
#endif

            return workstationInfo;
        }
    }
}
