using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.Modules.UpdateCheck.Messages
{
    public sealed class ApplicationUpdateAvailableMessage : PubSubMessageBase
    {
        public string Filepath { get; set; }

        public string Version { get; set; }

        public ApplicationUpdateAvailableMessage(string filepath, string version)
        {
            Filepath = filepath;
            Version = version;
        }
    }
}
