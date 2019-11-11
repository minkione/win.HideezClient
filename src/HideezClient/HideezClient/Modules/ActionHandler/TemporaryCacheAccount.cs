using HideezClient.Models;
using HideezClient.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Modules.ActionHandler
{
    class TemporaryCacheAccount : ITemporaryCacheAccount
    {
        public TemporaryCacheAccount(TimeSpan timeAvailableCash)
        {
            PasswordReqCache = new TemporaryCache<Account>(timeAvailableCash);
            OtpReqCache = new TemporaryCache<Account>(timeAvailableCash);
        }

        public TemporaryCache<Account> PasswordReqCache { get; set; }
        public TemporaryCache<Account> OtpReqCache { get; set; }

        public void Clear()
        {
            PasswordReqCache.Clear();
            OtpReqCache.Clear();
        }
    }
}
