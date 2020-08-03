using System;
using System.Threading.Tasks;

namespace HideezClient.Modules.ServiceProxy
{
    public interface IServiceProxy
    {
        bool IsConnected { get; }
    }
}
