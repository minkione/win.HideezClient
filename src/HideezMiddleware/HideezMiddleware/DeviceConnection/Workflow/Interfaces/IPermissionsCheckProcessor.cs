using System.Threading;

namespace HideezMiddleware.DeviceConnection.Workflow.Interfaces
{
    public interface IPermissionsCheckProcessor
    {
        /// <summary>
        /// Check if workstation is allowed to connect vaults. For additional security, this is also handled on server-side
        /// </summary>
        /// <exception cref="HideezException">Thrown if: 
        /// - Alarm is enabled on server: with code <see cref="HideezErrorCode.HesAlarm"/> 
        /// - Workstation is not approved: with code <see cref="HideezErrorCode.HesWorkstationNotApproved"/>
        /// </exception>
        void CheckPermissions();
    }
}
