using Hideez.SDK.Communication.Backup;
using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Messages.Dialogs.BackupPassword
{
    class SetProgressUIBackupPasswordMessage: PubSubMessageBase
    {
        public string Text { get; set; }

        public SetProgressUIBackupPasswordMessage(string text)
        {
            Text = text;
        }
    }
}
