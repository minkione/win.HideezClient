using Microsoft.Win32;

namespace HideezMiddleware
{
    public static class HideezClientRegistryRoot
    {
        public static RegistryKey GetRootRegistryKey()
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default)?
                .OpenSubKey("SOFTWARE")?
                .OpenSubKey("Hideez")?
                .OpenSubKey("Client");
        }
    }
}
