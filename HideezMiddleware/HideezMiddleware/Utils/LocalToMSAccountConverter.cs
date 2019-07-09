using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.Utils
{
    public static class LocalToMSAccountConverter
    {
        // USER_INFO_24
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct USER_INFO_24
        {
            public bool usri24_internet_identity;
            public int usri24_flags;
            public string usri24_internet_provider_name;
            public string usri24_internet_principal_name;
            public string usri24_user_sid;
        }

        // NetUserGetInfo - Returns to a struct Information about the specified user
        [DllImport("Netapi32.dll")]
        public extern static int NetUserGetInfo([MarshalAs(UnmanagedType.LPWStr)] string servername, [MarshalAs(UnmanagedType.LPWStr)] string username, int level, out IntPtr bufptr);

        // NetAPIBufferFree - Used to clear the Network buffer after NetUserEnum
        [DllImport("Netapi32.dll")]
        public extern static int NetApiBufferFree(IntPtr Buffer);

        public static string TryTransformToMS(string localAccountName)
        {
            string msName = String.Empty;

            IntPtr bufptr;
            int ntStatus = NetUserGetInfo(null, localAccountName, 24, out bufptr); // Get struct with info about user by his local username
            if (ntStatus == 0)
            {
                var usri24 = (USER_INFO_24)Marshal.PtrToStructure(bufptr, typeof(USER_INFO_24)); // Convert pointer to the structure
                msName = usri24.usri24_internet_principal_name;
            }
            NetApiBufferFree(bufptr);

            return msName;
        }
    }
}
