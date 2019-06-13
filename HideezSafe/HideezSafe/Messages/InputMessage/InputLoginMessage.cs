using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class InputLoginMessage : InputMessageBase
    {
        public InputLoginMessage(string[] devicesId) : base(devicesId)
        {
        }
    }
}
