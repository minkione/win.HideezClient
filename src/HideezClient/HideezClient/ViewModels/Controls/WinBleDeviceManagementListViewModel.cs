using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using HideezClient.Mvvm;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HideezClient.ViewModels.Controls
{
    public class WinBleDeviceManagementListViewModel : LocalizedObject
    {
        readonly IMetaPubSub _metaMessenger;
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(WinBleDeviceManagementListViewModel));


        public WinBleDeviceManagementListViewModel(IMetaPubSub metaMessenger)
        {
            _metaMessenger = metaMessenger;

            _metaMessenger.TrySubscribeOnServer<WinBleControllersCollectionChanged>(OnControllersCollectionChanged);
            try
            {
                _metaMessenger.PublishOnServer(new RefreshServiceInfoMessage());
            }
            catch (Exception) { } // Handle error in case we are not connected to server
        }

        bool firsttime = true;
        private Task OnControllersCollectionChanged(WinBleControllersCollectionChanged args)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (args.Controllers.Count() > 0 && !args.Controllers.First().IsConnected && args.Controllers.First().IsDiscovered)
                    {
                        if (firsttime)
                        {
                            firsttime = false;
                            await _metaMessenger.PublishOnServer(new ConnectDeviceRequestMessage(args.Controllers.First().Id));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log?.WriteLine(ex);
                }
            });
        }
    }
}
