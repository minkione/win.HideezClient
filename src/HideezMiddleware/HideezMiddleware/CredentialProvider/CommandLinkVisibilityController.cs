using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using HideezMiddleware.DeviceConnection;
using HideezMiddleware.DeviceConnection.Workflow;
using HideezMiddleware.Threading;
using System;
using System.Threading.Tasks;
using WinBle._10._0._18362;

namespace HideezMiddleware.CredentialProvider
{
    /// <summary>
    /// Controls visibility of command link in credential provider based on the state 
    /// of <see cref="AdvertisementIgnoreList"/> and <see cref="WinBleConnectionManager"/>
    /// </summary>
    public sealed class CommandLinkVisibilityController : Logger, IDisposable
    {
        readonly CredentialProviderProxy _cpProxy;
        readonly WinBleConnectionManager _winBleConnectionManager;
        readonly ConnectionFlowProcessor _connectionFlowProcessor;
        readonly SemaphoreQueue _visibilityUpdateSemaphore = new SemaphoreQueue(1, 1);

        public CommandLinkVisibilityController(CredentialProviderProxy cpProxy, WinBleConnectionManager connectionManager, ConnectionFlowProcessor connectionFlowProcessor, ILog log)
            : base(nameof(CommandLinkVisibilityController), log)
        {
            _cpProxy = cpProxy;
            _winBleConnectionManager = connectionManager;
            _connectionFlowProcessor = connectionFlowProcessor;

            _cpProxy.Connected += CredentialProviderProxy_Connected;

            _winBleConnectionManager.BondedControllerAdded += ConnectionManager_BondedControllerAdded;
            _winBleConnectionManager.BondedControllerRemoved += ConnectionManager_BondedControllerRemoved;
            
            _connectionFlowProcessor.Started += ConnectionFlowProcessor_Started;
            _connectionFlowProcessor.Finished += ConnectionFlowProcessor_Finished;
        }

        async void CredentialProviderProxy_Connected(object sender, EventArgs e)
        {
            await UpdateControlLinkVisibility();
        }

        async void ConnectionManager_BondedControllerAdded(object sender, ControllerAddedEventArgs e)
        {
            await UpdateControlLinkVisibility();
        }

        async void ConnectionManager_BondedControllerRemoved(object sender, ControllerRemovedEventArgs e)
        {
            await UpdateControlLinkVisibility();
        }

        async void ConnectionFlowProcessor_Started(object sender, string e)
        {
            await UpdateControlLinkVisibility();
        }

        async void ConnectionFlowProcessor_Finished(object sender, string e)
        {
            await UpdateControlLinkVisibility();
        }

        /// <summary>
        /// <para>Changes control link visibility in credential provider.
        /// Control link will be displayed if all conditions are true:</para>
        /// <br>- At least one device is paired through WinBle</br>
        /// <br>- Workflow is not running</br>
        /// <para>Otherwise, control link will be hidden.</para>
        /// </summary>
        async Task UpdateControlLinkVisibility()
        {
            await _visibilityUpdateSemaphore.WaitAsync();
            try
            {
                // No reason sending anything if credential provider is not connected
                if (_cpProxy.IsConnected)
                {
                    if (_winBleConnectionManager.BondedControllers.Count == 0 || _connectionFlowProcessor.IsRunning)
                        await _cpProxy.HideCommandLink();
                    else
                        await _cpProxy.ShowCommandLink();
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
            finally
            {
                _visibilityUpdateSemaphore.Release();
            }
        }

        public void Dispose()
        {
            _cpProxy.Connected -= CredentialProviderProxy_Connected;

            _winBleConnectionManager.BondedControllerAdded -= ConnectionManager_BondedControllerAdded;
            _winBleConnectionManager.BondedControllerRemoved -= ConnectionManager_BondedControllerRemoved;

            _connectionFlowProcessor.Started -= ConnectionFlowProcessor_Started;
            _connectionFlowProcessor.Finished -= ConnectionFlowProcessor_Finished;
        }
    }
}
