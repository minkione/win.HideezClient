using HideezSafe.HideezServiceReference;
using System;
using System.Threading.Tasks;

namespace HideezSafe.Modules.ServiceProxy
{
    public interface IServiceProxy
    {
        bool IsConnected { get; }

        event EventHandler Connected;
        event EventHandler Disconnected;

        IHideezService GetService();

        Task<bool> ConnectAsync();
        Task DisconnectAsync();
    }
}
