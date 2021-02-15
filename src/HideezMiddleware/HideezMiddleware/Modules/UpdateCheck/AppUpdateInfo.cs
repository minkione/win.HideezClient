using System;

namespace HideezMiddleware.Modules.UpdateCheck
{
    public class AppUpdateInfo
    {
        public Version Version { get; set; }
        public string Url { get; set; }
        public string Changelog { get; set; }
    }
}
