using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class InputOtpMessage : InputMessageBase
    {
        public InputOtpMessage(string[] devicesId) : base(devicesId)
        {
        }
    }
}
