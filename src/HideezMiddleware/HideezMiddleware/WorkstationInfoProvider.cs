using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
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
        private readonly IPEndPoint endPoint;

        public WorkstationInfoProvider(string hostNameOrAddress, ILog log)
        {
            this.log = log;
            try
            {
                if (UrlUtils.TryGetUri(hostNameOrAddress, out Uri uri))
                {
                    IPAddress hostAddress = Dns.GetHostEntry(uri.Host).AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    endPoint = new IPEndPoint(hostAddress, uri.Port);
                }
                else
                {
                    log?.WriteLine(nameof(WorkstationInfoProvider), $"{nameof(hostNameOrAddress)} not valid format.", LogErrorSeverity.Error);
                }
            }
            catch (Exception ex)
            {
                log?.WriteLine(nameof(WorkstationInfoProvider), ex);
            }
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

                if (endPoint != null)
                {
                    IPAddress localIP = await WorkstationHelper.GetLocalIPAddressAsync(endPoint);
                    PhysicalAddress mac = WorkstationHelper.GetCurrentMAC(localIP);

                    workstationInfo.IP = localIP.ToString();
                    workstationInfo.MAC = mac.ToString();
                }
                else
                {
                    log?.WriteLine(nameof(WorkstationInfoProvider), $"{nameof(endPoint)} is null or none.", LogErrorSeverity.Error);
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
