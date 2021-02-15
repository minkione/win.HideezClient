using System;

namespace HideezMiddleware.Modules.UpdateCheck
{
    internal sealed class CachedUpdateInfo
    {
        public string Filepath { get; }

        public Version Version { get; }

        public CachedUpdateInfo(string filepath, Version version)
        {
            Filepath = filepath;
            Version = version;
        }
    }
}
