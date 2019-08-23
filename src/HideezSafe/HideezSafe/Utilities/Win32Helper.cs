using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Utilities
{
    class Win32Helper
    {
        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}
