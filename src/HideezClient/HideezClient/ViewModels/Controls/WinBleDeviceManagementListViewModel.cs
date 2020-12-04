using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using HideezClient.Mvvm;
using HideezMiddleware.IPC.IncommingMessages;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HideezClient.ViewModels.Controls
{
    public class WinBleDeviceManagementListViewModel : LocalizedObject
    {
        readonly IMetaPubSub _metaMessenger;
        readonly Logger _log = LogManager.GetCurrentClassLogger(nameof(WinBleDeviceManagementListViewModel));
        readonly Dictionary<string, WinBleControllerStateViewModel> _controllersVmDictionary = new Dictionary<string, WinBleControllerStateViewModel>();
        readonly object _handlerLock = new object();
        readonly object _dictionaryLock = new object();
        

        public IReadOnlyCollection<WinBleControllerStateViewModel> Controllers
        {
            get
            {
                lock (_dictionaryLock)
                {
                    return _controllersVmDictionary.Values.ToList()
                        .OrderByDescending(c => c.IsConnected)
                        .ThenByDescending(c => c.IsDiscovered)
                        .ToList()
                        .AsReadOnly();
                }
            }
        }

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

        private Task OnControllersCollectionChanged(WinBleControllersCollectionChanged args)
        {
            lock (_handlerLock)
            {
                Task.Run(() =>
                {
                    lock (_dictionaryLock)
                    {
                        var missingKeys = _controllersVmDictionary.Keys.Except(args.Controllers.Select(controller => controller.Id));
                        foreach (var key in missingKeys)
                            _controllersVmDictionary.Remove(key);

                        foreach (var controllerDto in args.Controllers)
                        {
                            if (_controllersVmDictionary.ContainsKey(controllerDto.Id))
                                _controllersVmDictionary[controllerDto.Id].FromDto(controllerDto);
                            else
                            {
                                var newVm = new WinBleControllerStateViewModel(_metaMessenger);
                                newVm.FromDto(controllerDto);
                                _controllersVmDictionary[controllerDto.Id] = newVm;
                            }
                        }

                    }
                    
                    NotifyPropertyChanged(nameof(Controllers));
                });
            }
            return Task.CompletedTask;
        }
    }
}
