using Hideez.SDK.Communication.FW;
using Meta.Lib.Modules.PubSub;

namespace DeviceMaintenance.Messages
{
    public class EnterBootResponse : PubSubMessageBase
    {
        public FirmwareImageUploader ImageUploader { get; }

        public EnterBootResponse(FirmwareImageUploader imageUploader)
        {
            ImageUploader = imageUploader;
        }
    }
}
