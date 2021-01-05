using Hideez.SDK.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.IPC.DTO
{
    public class DeviceDTOFactory
    {
        readonly object _locker = new object();
        readonly Dictionary<string, long> _countersDictionary = new Dictionary<string, long>();

        public DeviceDTO Create(IDevice device)
        {
            lock (_locker)
            {
                if (_countersDictionary.TryGetValue(device.Id, out long counter))
                    _countersDictionary[device.Id] = ++counter;
                else _countersDictionary.Add(device.Id, ++counter);

                return new DeviceDTO(device)
                {
                    Counter = counter
                };
            }
        }

        public void ResetCounter(IDevice device)
        {
            if (_countersDictionary.ContainsKey(device.Id))
                _countersDictionary.Remove(device.Id);
        }
    }
}
