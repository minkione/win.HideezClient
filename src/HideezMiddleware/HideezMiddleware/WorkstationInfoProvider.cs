using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.Workstation;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class WorkstationInfoProvider : Logger, IWorkstationInfoProvider
    {
        readonly IPEndPoint endPoint;

        public WorkstationInfoProvider(string hostNameOrAddress, ILog log)
            : base(nameof(WorkstationInfoProvider), log)
        {
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
                    WriteLine(ex);
                    Debug.Assert(false, "An exception occured while querrying workstation operating system");
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
                    WriteLine($"{nameof(endPoint)} is null or none.", LogErrorSeverity.Error);
                }

                workstationInfo.Users = await WorkstationHelper.GetAllUserNamesAsync();
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
