using Hideez.SDK.Communication.Log;
using Microsoft.Win32;
using System;

namespace HideezMiddleware.Workstation
{
    public class WorkstationIdProvider : Logger, IWorkstationIdProvider
    {
        const string REG_ID_NAME = "id";
        readonly RegistryKey _registryRootKey;

        public WorkstationIdProvider(RegistryKey rootKey, ILog log)
            : base(nameof(WorkstationIdProvider), log)
        {
            _registryRootKey = rootKey ?? throw new ArgumentNullException(nameof(rootKey));
        }

        public void SaveWorkstationId(string id)
        {
            _registryRootKey.SetValue(REG_ID_NAME, id, RegistryValueKind.String);
        }

        public string GetWorkstationId()
        {
            var id = _registryRootKey.GetValue(REG_ID_NAME) as string;
            return id;
        }
    }
}
