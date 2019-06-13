using HideezSafe.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezSafe.Modules.ActionHandler
{
    interface ITemporaryCacheAccount
    {
        TemporaryCache<Account> OtpReqCache { get; set; }
        TemporaryCache<Account> PasswordReqCache { get; set; }

        void Clear();
    }
}
