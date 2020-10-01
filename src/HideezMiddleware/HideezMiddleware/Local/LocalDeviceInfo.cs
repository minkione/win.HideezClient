using Hideez.SDK.Communication.HES.DTO;

namespace HideezMiddleware.Local
{
    public class LocalDeviceInfo
    {
        public string SerialNo { get; set; } = string.Empty;
        public string Mac { get; set; } = string.Empty;
        public string RFID { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;

        public LocalDeviceInfo() { }

        public LocalDeviceInfo(HwVaultShortInfoFromHesDto dto)
        {
            SerialNo = dto.VaultSerialNo;
            Mac = dto.VaultMac;
            // TODO: Add RFID to device info cache
            OwnerName = dto.OwnerName;
            OwnerEmail = dto.OwnerEmail;
        }
    }
}
