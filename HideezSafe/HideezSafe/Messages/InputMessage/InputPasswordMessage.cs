using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Messages
{
    class InputPasswordMessage : InputMessageBase
    {
        public InputPasswordMessage(string[] devicesId) : base(devicesId)
        {
        }
    }
}
