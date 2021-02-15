using System;

namespace HideezMiddleware.DeviceConnection
{
    public interface IConnectionProcessor
    {
        void Start();

        void Stop();
    }
}