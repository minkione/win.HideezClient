using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using HideezMiddleware.DeviceConnection.Workflow.Interfaces;
using HideezMiddleware.Settings;
using System.Threading;

namespace HideezMiddleware.DeviceConnection.Workflow
{
    public class PermissionsCheckProcessor: IPermissionsCheckProcessor
    {
        readonly IHesAccessManager _hesAccessManager;
        readonly ISettingsManager<ServiceSettings> _serviceSettingsManager;

        public PermissionsCheckProcessor(IHesAccessManager hesAccessManager, ISettingsManager<ServiceSettings> serviceSettingsManager)
        {
            _hesAccessManager = hesAccessManager;
            _serviceSettingsManager = serviceSettingsManager;
        }

        /// <summary>
        /// Check if workstation is allowed to connect devices
        /// </summary>
        /// <exception cref="HideezException">Thrown with code <see cref="HideezErrorCode.HesAlarm"/> when alarm is turned on</exception>
        /// <exception cref="HideezException">Thrown with code <see cref="HideezErrorCode.HesWorkstationNotApproved"/> when workstation is not approved on HES</exception>
        public void CheckPermissions(CancellationTokenSource cts)
        {
            if (_serviceSettingsManager.Settings.AlarmTurnOn)
            {
                cts?.Cancel();
                throw new HideezException(HideezErrorCode.HesAlarm);
            }    

            if (!_hesAccessManager.HasAccessKey())
                throw new HideezException(HideezErrorCode.HesWorkstationNotApproved);
        }
    }
}
