using System.Runtime.InteropServices;

namespace Locker
{
    class Program
    {
        static void Main(string[] args)
        {
            LockWorkStation();
        }

        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();
    }
}
