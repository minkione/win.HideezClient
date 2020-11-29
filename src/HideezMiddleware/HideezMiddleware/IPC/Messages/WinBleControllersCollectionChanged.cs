using Hideez.SDK.Communication.Interfaces;
using HideezMiddleware.IPC.DTO;
using Meta.Lib.Modules.PubSub;

namespace HideezMiddleware.IPC.Messages
{
    public sealed class WinBleControllersCollectionChanged : PubSubMessageBase
    {
        public WinBleControllerStateDTO[] Controllers { get; }

        public WinBleControllersCollectionChanged(WinBleControllerStateDTO[] controllers)
        {
            Controllers = controllers;
        }
    }
}
