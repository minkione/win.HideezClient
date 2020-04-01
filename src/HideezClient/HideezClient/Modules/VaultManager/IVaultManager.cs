using HideezClient.Models;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace HideezClient.Modules.VaultManager
{
    public interface IVaultManager
    {
        event NotifyCollectionChangedEventHandler DevicesCollectionChanged;

        IEnumerable<HardwareVaultModel> Vaults { get; }
    }
}
