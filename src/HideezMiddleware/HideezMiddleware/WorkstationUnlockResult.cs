namespace HideezMiddleware
{
    public class WorkstationUnlockResult
    {
        /// <summary>
        /// Tells if workstation unlock was successful
        /// </summary>
        public bool IsSuccessful { get; set; } = false;

        /// <summary>
        /// Name of account used for workstation unlock
        /// </summary>
        public string AccountName { get; set; } = string.Empty;

        /// <summary>
        /// Login used for workstation unlock
        /// </summary>
        public string AccountLogin { get; set; } = string.Empty;

        /// <summary>
        /// Mac of the device that was used for unlock
        /// </summary>
        public string DeviceMac { get; set; } = string.Empty;
    }
}
