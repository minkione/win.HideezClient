using System;

namespace HideezMiddleware.DeviceConnection
{
    public interface IConnectionProcessor
    {
        event EventHandler<WorkstationUnlockResult> WorkstationUnlockPerformed;

        void Start();

        void Stop();
    }
}