using HideezClient.Dialogs;
using HideezClient.Messages.Dialogs;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Modules.HideDialogsAdapter
{
    public class HideDialogsAdapter: IHideDialogsAdapter
    {
        private readonly IMetaPubSub _metaMessenger;

        public HideDialogsAdapter(IMetaPubSub metaMessenger)
        {
            _metaMessenger = metaMessenger;
            _metaMessenger.TrySubscribeOnServer<HideActivationCodeUi>(HideActivationDialogAsync);
        }

        async Task HideActivationDialogAsync(HideActivationCodeUi message)
        {
            await _metaMessenger.Publish(new HideDialogMessage(typeof(ActivationDialog)));
        }
    }
}
