using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Messages
{
    class UnlockWorkstationMessage
    {
        public bool IsDisabledLock { get; }

        public UnlockWorkstationMessage(bool isDisabledLock)
        {
            IsDisabledLock = isDisabledLock;
        }
    }
}
