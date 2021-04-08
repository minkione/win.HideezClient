using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.FwUpdateCheck.Messages
{
    public class FwUpdateAvailableMessage : PubSubMessageBase
    {
        public string FilePath { get; set; }

        public string Version { get; set; }

        public FwUpdateAvailableMessage(string filepath, string version)
        {
            FilePath = filepath;
            Version = version;
        }
    }
}
