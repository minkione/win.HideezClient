using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    abstract class InputMessageBase
    {
        public InputMessageBase(string[] devicesId)
        {
            this.DevicesId = devicesId;
        }

        public string[] DevicesId { get; }
    }
}
