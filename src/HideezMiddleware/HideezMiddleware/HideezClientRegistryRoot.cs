using Microsoft.Win32;

namespace HideezMiddleware
{
    public static class HideezClientRegistryRoot
    {
        public static string RootKeyPath { get; } = "HKLM\\SOFTWARE\\Hideez\\Client";

        /// <exception cref="System.UnauthorizedAccessException">The RegistryKey is read-only, and cannot be written to; for example, the key has not been opened with write access.</exception>
        /// <exception cref="System.Security.SecurityException">The user does not have the permissions required to create or modify registry keys.</exception>
        public static RegistryKey GetRootRegistryKey(bool writable = false)
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)?
                .OpenSubKey("SOFTWARE")?
                .OpenSubKey("Hideez")?
                .OpenSubKey("Client", writable);
        }
    }
}
