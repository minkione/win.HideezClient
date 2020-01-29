namespace HideezMiddleware.Local
{
    public interface ILocalDeviceInfoCache
    {
        void SaveLocalInfo(LocalDeviceInfo info);

        LocalDeviceInfo GetLocalInfo(string deviceMac);

        void RemoveLocalInfo(string deviceMac);
    }
}
