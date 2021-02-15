using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Messages
{
    public class SendMasterPasswordMessage : PubSubMessageBase
    {
        public string DeviceId { get; }

        public byte[] Password { get; }

        public byte[] OldPassword { get; }

        public SendMasterPasswordMessage(string deviceId, byte[] password, byte[] oldPassword = null)
        {
            DeviceId = deviceId;
            Password = password;
            OldPassword = oldPassword;
        }
    }
}
