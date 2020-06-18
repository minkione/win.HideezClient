using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Messages
{
    class ProximityLockMessage
    {
        public bool IsDisabledLock { get; }

        public ProximityLockMessage(bool isDisabledLock)
        {
            IsDisabledLock = isDisabledLock;
        }
    }
}
