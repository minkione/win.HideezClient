using Hideez.SDK.Communication.HES.Client;
using Meta.Lib.Modules.PubSub;
using System.Collections.Generic;

namespace HideezMiddleware.Modules.Hes.Messages
{
    public sealed class HesAppConnection_HubProximitySettingsArrivedMessage : PubSubMessageBase
    {
        public object Sender { get; }
        public IReadOnlyList<DeviceProximitySettings> Settings { get; }

        public HesAppConnection_HubProximitySettingsArrivedMessage(object sender, IReadOnlyList<DeviceProximitySettings> settings)
        {
            Sender = sender;
            Settings = settings;
        }
    }
}