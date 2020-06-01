using Hideez.SDK.Communication.FW;

namespace DeviceMaintenance.Messages
{
    public class EnterBootResponse : MessageBase
    {
        public FirmwareImageUploader ImageUploader { get; }

        public EnterBootResponse(FirmwareImageUploader imageUploader)
        {
            ImageUploader = imageUploader;
        }
    }
}
