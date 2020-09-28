using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Utils;
using HideezMiddleware.Localize;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class UnlockProcessor : Logger
    {
        readonly UiProxyManager _ui;
        readonly IWorkstationUnlocker _workstationUnlocker;

        public UnlockProcessor(UiProxyManager ui, IWorkstationUnlocker workstationUnlocker, ILog log)
            : base(nameof(UnlockProcessor), log)
        {
            _ui = ui;
            _workstationUnlocker = workstationUnlocker;
        }

        public async Task UnlockWorkstation(IDevice device, string flowId, Action<WorkstationUnlockResult> onUnlockAttempt, CancellationToken ct)
        {
	        ct.ThrowIfCancellationRequested();

            await TryUnlockWorkstation(device, flowId, onUnlockAttempt);
        }

        async Task TryUnlockWorkstation(IDevice device, string flowId, Action<WorkstationUnlockResult> onUnlockAttempt)
        {
            var result = new WorkstationUnlockResult();

            await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Unlock.ReadingCredentials"], device.Mac);
            var credentials = await GetCredentials(device);

            // send credentials to the Credential Provider to unlock the PC
            await _ui.SendNotification(TranslationSource.Instance["ConnectionFlow.Unlock.Unlocking"], device.Mac);
            result.IsSuccessful = await _workstationUnlocker
                .SendLogonRequest(credentials.Login, credentials.Password, credentials.PreviousPassword);

            result.AccountName = credentials.Name;
            result.AccountLogin = credentials.Login;
            result.DeviceMac = device.Mac;
            result.FlowId = flowId;

            onUnlockAttempt?.Invoke(result);

            if (!result.IsSuccessful)
                throw new WorkstationUnlockFailedException(); // Abort connection flow
        }

        async Task<Credentials> GetCredentials(IDevice device)
        {
            ushort primaryAccountKey = await DevicePasswordManager.GetPrimaryAccountKey(device);
            var credentials = await GetCredentials(device, primaryAccountKey);
            return credentials;
        }

        async Task<Credentials> GetCredentials(IDevice device, ushort key)
        {
            Credentials credentials;

            if (key == 0)
            {
                var str = await device.ReadStorageAsString(
                    (byte)StorageTable.BondVirtualTable1,
                    (ushort)BondVirtualTable1Item.PcUnlockCredentials);

                if (str != null)
                {
                    var parts = str.Split('\n');
                    if (parts.Length >= 2)
                    {
                        credentials.Login = parts[0];
                        credentials.Password = parts[1];
                    }
                    if (parts.Length >= 3)
                    {
                        credentials.PreviousPassword = parts[2];
                    }
                }

                if (credentials.IsEmpty)
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.Unlock.Error.NoCredentials"]);
            }
            else
            {
                // get the account name, login and password from the Hideez Key
                credentials.Name = await device.ReadStorageAsString((byte)StorageTable.Accounts, key);
                credentials.Login = await device.ReadStorageAsString((byte)StorageTable.Logins, key);
                credentials.Password = await device.ReadStorageAsString((byte)StorageTable.Passwords, key);
                credentials.PreviousPassword = ""; //todo

                // Todo: Uncomment old message when primary account key sync is fixed
                //if (credentials.IsEmpty)
                //    throw new Exception($"Cannot read login or password from the vault '{device.SerialNo}'");
                if (credentials.IsEmpty)
                    throw new WorkflowException(TranslationSource.Instance["ConnectionFlow.Unlock.Error.NoCredentials"]);
            }

            return credentials;
        }
    }
}
