using HideezMiddleware.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware
{
    public class WorkstationInfoHelper
    {
        private class WorkstationInfoImpl : WorkstationInfo
        {
            public async Task InitAsync(IPAddress allowedIP)
            {
#if DEBUG
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
#endif

                try
                {
                    AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                    MachineName = Environment.MachineName;
                    Domain = Environment.UserDomainName;

                    try
                    {
                        RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion");
                        OsName = (string)registryKey.GetValue("ProductName");
                        OSVersion = (string)registryKey.GetValue("ReleaseId");
                        OsBuild = $"{registryKey.GetValue("CurrentBuild")}.{registryKey.GetValue("UBR")}";
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        Debug.Assert(false);
                    }

                    IPAddress localIP = await GetLocalIPAddressAsync(allowedIP);
                    PhysicalAddress mac = GetCurrentMAC(localIP);

                    IPAddress = localIP.ToString();
                    MACAddress = mac.ToString();

                    WindowsUserAccounts = await GetAllUserNamesAsync();
                }
                catch (Exception ex)
                {
                    HandlerError(ex);
                }

#if DEBUG
                stopwatch.Stop();
                var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
#endif
            }
        }

        public static async Task<WorkstationInfo> GetWorkstationInfoAsync(IPAddress allowedIP = null)
        {
            if (allowedIP == null || allowedIP == IPAddress.None)
            {
                allowedIP = Dns.GetHostAddresses("www.google.com").FirstOrDefault();
            }

            WorkstationInfoImpl workstationInfo = new WorkstationInfoImpl();
            await workstationInfo.InitAsync(allowedIP);

            return workstationInfo;
        }

        public static PhysicalAddress GetCurrentMAC(IPAddress localIPAddres)
        {
            PhysicalAddress physicalAddress = PhysicalAddress.None;
            try
            {
                NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                if (allNetworkInterfaces.Length > 0)
                {
                    foreach (NetworkInterface interface2 in allNetworkInterfaces)
                    {
                        UnicastIPAddressInformationCollection unicastAddresses = interface2.GetIPProperties().UnicastAddresses;
                        if ((unicastAddresses != null) && (unicastAddresses.Count > 0))
                        {
                            for (int i = 0; i < unicastAddresses.Count; i++)
                            {
                                if (unicastAddresses[i].Address.Equals(localIPAddres))
                                {
                                    physicalAddress = interface2.GetPhysicalAddress();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandlerError(ex);
            }

            return physicalAddress;
        }

        public static async Task<IPAddress> GetLocalIPAddressAsync(IPAddress allowedIP)
        {
            IPAddress address = IPAddress.None;
            TcpClient client = new TcpClient();
            try
            {
                await client.Client.ConnectAsync(new IPEndPoint(allowedIP, 80));
                while (!client.Connected)
                {
                    await Task.Delay(100);
                }
                address = ((IPEndPoint)client.Client.LocalEndPoint).Address;
            }
            catch (Exception ex)
            {
                HandlerError(ex);
            }
            finally
            {
                if (client != null)
                {
                    try
                    {
                        client.Client.Disconnect(false);
                        client.Close();
                    }
                    catch { }
                }
            }

            return address;
        }

        public static Task<string[]> GetAllUserNamesAsync()
        {
            return Task.Run(new Func<string[]>(GetAllUserNames));
        }

        public static string[] GetAllUserNames()
        {
            List<string> result = new List<string>();

            // Get all "real" local usernames
            SelectQuery query = new SelectQuery("Select * from Win32_UserAccount Where LocalAccount = True");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            var localUsers = searcher.Get().Cast<ManagementObject>().Where(
                u => (bool)u["LocalAccount"] == true &&
                     (bool)u["Disabled"] == false &&
                     (bool)u["Lockout"] == false &&
                     int.Parse(u["SIDType"].ToString()) == 1 &&
                     u["Name"].ToString() != "HomeGroupUser$");

            // Try to get MS Account for each local username and if found use it instead of local username
            foreach (ManagementObject user in localUsers)
            {
                string msName = LocalToMSAccountConverter.TryTransformToMS(user["Name"] as string);

                if (!String.IsNullOrWhiteSpace(msName))
                    result.Add(@"MicrosoftAccount\" + msName);
                else
                    result.Add(new SecurityIdentifier(user["SID"].ToString()).Translate(typeof(NTAccount)).ToString());
            }

            return result.ToArray();
        }

        private static void HandlerError(Exception ex)
        {
            Debug.WriteLine(ex);
            Debug.Assert(false);
        }
    }
}
