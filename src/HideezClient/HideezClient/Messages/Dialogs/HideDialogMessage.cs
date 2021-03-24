using Meta.Lib.Modules.PubSub;
using System;

namespace HideezClient.Messages.Dialogs
{
    public class HideDialogMessage: PubSubMessageBase
    {
        public Type DialogType { get; }

        public HideDialogMessage(Type dialogType)
        {
            DialogType = dialogType;
        }
    }
}
