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
            PasswordReqCache = new TemporaryCache<AccountModel>(timeAvailableCash);
            OtpReqCache = new TemporaryCache<AccountModel>(timeAvailableCash);
        }

        public TemporaryCache<AccountModel> PasswordReqCache { get; set; }
        public TemporaryCache<AccountModel> OtpReqCache { get; set; }

        public void Clear()
        {
            PasswordReqCache.Clear();
            OtpReqCache.Clear();
        }
    }
}
