using System;
using System.Runtime.InteropServices;

namespace HideezMiddleware
{
    public partial class WorkstationHelper
    {
        const Int32 FALSE = 0;

        static readonly IntPtr WTS_CURRENT_SERVER = IntPtr.Zero;

        const Int32 WTS_SESSIONSTATE_LOCK = 0;
        const Int32 WTS_SESSIONSTATE_UNLOCK = 1;

        static bool _is_win7 = (Environment.OSVersion.Platform == PlatformID.Win32NT && 
            Environment.OSVersion.Version.Major == 6 && 
            Environment.OSVersion.Version.Minor == 1);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto)]
        static extern Int32 WTSQuerySessionInformation(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] uint SessionId,
            [MarshalAs(UnmanagedType.U4)] WTS_INFO_CLASS WTSInfoClass,
            out IntPtr ppBuffer,
            [MarshalAs(UnmanagedType.U4)] out uint pBytesReturned
        );

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto)]
        static extern void WTSFreeMemoryEx(
            WTS_TYPE_CLASS WTSTypeClass,
            IntPtr pMemory,
            uint NumberOfEntries
        );

        enum WTS_INFO_CLASS
        {
            WTSInitialProgram = 0,
            WTSApplicationName = 1,
            WTSWorkingDirectory = 2,
            WTSOEMId = 3,
            WTSSessionId = 4,
            WTSUserName = 5,
            WTSWinStationName = 6,
            WTSDomainName = 7,
            WTSConnectState = 8,
            WTSClientBuildNumber = 9,
            WTSClientName = 10,
            WTSClientDirectory = 11,
            WTSClientProductId = 12,
            WTSClientHardwareId = 13,
            WTSClientAddress = 14,
            WTSClientDisplay = 15,
            WTSClientProtocolType = 16,
            WTSIdleTime = 17,
            WTSLogonTime = 18,
            WTSIncomingBytes = 19,
            WTSOutgoingBytes = 20,
            WTSIncomingFrames = 21,
            WTSOutgoingFrames = 22,
            WTSClientInfo = 23,
            WTSSessionInfo = 24,
            WTSSessionInfoEx = 25,
            WTSConfigInfo = 26,
            WTSValidationInfo = 27,
            WTSSessionAddressV4 = 28,
            WTSIsRemoteSession = 29
        }

        enum WTS_TYPE_CLASS
        {
            WTSTypeProcessInfoLevel0,
            WTSTypeProcessInfoLevel1,
            WTSTypeSessionInfoLevel1
        }

        enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        public enum LockState
        {
            Unknown,
            Locked,
            Unlocked
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WTSINFOEX
        {
            public uint Level;
            public uint Reserved; /* I have observed the Data field is pushed down by 4 bytes so i have added this field as padding. */
            public WTSINFOEX_LEVEL Data;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WTSINFOEX_LEVEL
        {
            public WTSINFOEX_LEVEL1 WTSInfoExLevel1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WTSINFOEX_LEVEL1
        {
            public uint SessionId;
            public WTS_CONNECTSTATE_CLASS SessionState;
            public Int32 SessionFlags;

            /* I can't figure out what the rest of the struct should look like but as i don't need anything past the SessionFlags i'm not going to. */

        }

        public static LockState GetSessionLockState(uint session_id)
        {

            Int32 result = WTSQuerySessionInformation(
                WTS_CURRENT_SERVER,
                session_id,
                WTS_INFO_CLASS.WTSSessionInfoEx,
                out IntPtr ppBuffer,
                out uint pBytesReturned
            );

            if (result == FALSE)
                return LockState.Unknown;

            var session_info_ex = Marshal.PtrToStructure<WTSINFOEX>(ppBuffer);

            if (session_info_ex.Level != 1)
            {
                WTSFreeMemory(ppBuffer);
                return LockState.Unknown;
            }

            var lock_state = session_info_ex.Data.WTSInfoExLevel1.SessionFlags;
            WTSFreeMemory(ppBuffer);

            if (_is_win7)
            {
                /* Ref: https://msdn.microsoft.com/en-us/library/windows/desktop/ee621019(v=vs.85).aspx
                    * Windows Server 2008 R2 and Windows 7:  Due to a code defect, the usage of the WTS_SESSIONSTATE_LOCK
                    * and WTS_SESSIONSTATE_UNLOCK flags is reversed. That is, WTS_SESSIONSTATE_LOCK indicates that the
                    * session is unlocked, and WTS_SESSIONSTATE_UNLOCK indicates the session is locked.
                    * */
                switch (lock_state)
                {
                    case WTS_SESSIONSTATE_LOCK:
                        return LockState.Unlocked;

                    case WTS_SESSIONSTATE_UNLOCK:
                        return LockState.Locked;

                    default:
                        return LockState.Unknown;
                }
            }
            else
            {
                switch (lock_state)
                {
                    case WTS_SESSIONSTATE_LOCK:
                        return LockState.Locked;

                    case WTS_SESSIONSTATE_UNLOCK:
                        return LockState.Unlocked;

                    default:
                        return LockState.Unknown;
                }
            }
        }

        public static LockState GetCurrentSessionLockState()
        {
            var sid = GetSessionId();
            return GetSessionLockState(sid);
        }
    }
}
