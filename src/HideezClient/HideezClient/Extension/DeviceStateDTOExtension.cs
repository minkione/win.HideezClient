using Hideez.SDK.Communication.Device;
using HideezClient.HideezServiceReference;

namespace HideezClient.Extension
{
    static class DeviceStateDTOExtension
    {
        public static DeviceState ToDeviceState(this DeviceStateDTO dto)
        {
            return new DeviceState()
            {
                Battery = dto.Battery,
                Rssi = dto.Rssi,
                PinAttemptsRemain = dto.PinAttemptsRemain,
                StorageUpdateCounter = dto.StorageUpdateCounter,
                Button = (Hideez.SDK.Communication.ButtonPressCode)dto.Button,
                AccessLevel = dto.AccessLevel.ToAccessLevel(),
            };
        }
    }
}
