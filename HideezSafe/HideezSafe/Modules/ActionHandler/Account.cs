using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules.ActionHandler
{
    public class Account
    {
        public Account()
        {
            throw new NotImplementedException();
        }

        public string DeviceId { get; }
        public ushort Key { get; }
        public string Login { get; }
        public bool HasOtpSecret { get; }
    }
}
