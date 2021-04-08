using Hideez.SDK.Communication.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;

namespace HideezMiddleware.Utils.WorkstationHelper
{
    public partial class WorkstationInformationHelper
    {
        static readonly string SYSTEM_SESSION = "SYSTEM";

        [DllImport("Wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation(IntPtr hServer, uint sessionId, WtsInfoClass wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);

        [DllImport("Wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pointer);

        [DllImport("kernel32.dll")]
        static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll")]
        static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

        [DllImport("wtsapi32.dll")]
        static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll")]
        static extern Int32 WTSEnumerateSessions(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] Int32 Reserved,
            [MarshalAs(UnmanagedType.U4)] Int32 Version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

        private enum WtsInfoClass
        {
            WTSUserName = 5,
            WTSDomainName = 7,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public uint SessionID;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        public static ILog Log { get; set; }

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
                        if (unicastAddresses != null && unicastAddresses.Count > 0)
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
                Log?.WriteLine(nameof(WorkstationInformationHelper), ex);
                Debug.Assert(false);
            }

            return physicalAddress;
        }

        public static async Task<IPAddress> GetLocalIPAddressAsync(IPEndPoint endPoint)
        {
            IPAddress address = IPAddress.None;
            TcpClient client = new TcpClient();
            try
            {
                await client.Client.ConnectAsync(endPoint);
                while (!client.Connected)
                {
                    await Task.Delay(100);
                }
                address = ((IPEndPoint)client.Client.LocalEndPoint).Address;
            }
            catch (Exception ex)
            {
                Log?.WriteLine(nameof(WorkstationInformationHelper), ex);
                Debug.Assert(false);
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

                if (!string.IsNullOrWhiteSpace(msName))
                    result.Add(@"MicrosoftAccount\" + msName);
                else
                    result.Add(new SecurityIdentifier(user["SID"].ToString()).Translate(typeof(NTAccount)).ToString());
            }

            return result.ToArray();
        }

        public static string GetSessionName(uint sessionId)
        {
            string username = SYSTEM_SESSION;
            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WTSUserName, out IntPtr buffer, out int strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
            }
            return username;
        }

        public static uint GetSessionId()
        {
            IntPtr serverHandle = WTSOpenServer("localhost");
            uint activeSessionId = 0;

            try
            {
                int sessionInfoSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                IntPtr SessionInfoPtr = IntPtr.Zero;
                int sessionCount = 0;

                if (WTSEnumerateSessions(serverHandle, 0, 1, ref SessionInfoPtr, ref sessionCount) != 0)
                {
                    IntPtr enumeratedSession = SessionInfoPtr;

                    for (int i = 0; i < sessionCount; i++)
                    {
                        WTS_SESSION_INFO sessionInfo = (WTS_SESSION_INFO)Marshal.PtrToStructure(enumeratedSession, typeof(WTS_SESSION_INFO));
                        enumeratedSession = IntPtr.Add(enumeratedSession, sessionInfoSize);

                        if (sessionInfo.State == WTS_CONNECTSTATE_CLASS.WTSActive)
                        {
                            var sessionName = GetSessionName(sessionInfo.SessionID);
                            if (sessionName != SYSTEM_SESSION)
                            {
                                activeSessionId = sessionInfo.SessionID;
                                break;
                            }
                        }
                    }

                    WTSFreeMemory(SessionInfoPtr);
                }

                return activeSessionId;
            }
            finally
            {
                WTSCloseServer(serverHandle);
            }
        }
    }
}
