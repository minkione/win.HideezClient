using Hideez.SDK.Communication.Log;
using HideezMiddleware.IPC.Messages;
using Meta.Lib.Modules.PubSub;
using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace HideezClient.Modules.BleDeviceUnpairHelper
{
    sealed class BleDeviceUnpairHelper : Logger
    {
        readonly IMetaPubSub _messenger;

        public BleDeviceUnpairHelper(IMetaPubSub messenger, ILog log) 
            : base(nameof(BleDeviceUnpairHelper), log)
        {
            _messenger = messenger;

            _messenger?.TrySubscribeOnServer<UnpairDeviceMessage>(OnUnpairDeviceMessage);
        }

        async Task OnUnpairDeviceMessage(UnpairDeviceMessage msg)
        {
            WriteLine($"Unpair device message received: {msg.Id}");
            try
            {
                var info = await DeviceInformation.CreateFromIdAsync(msg.Id);
                if (info != null)
                {
                    var unpairResult = await info.Pairing.UnpairAsync();
                    WriteLine($"Unpair vault ({msg.Id}): {unpairResult.Status}");
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }
    }
}
