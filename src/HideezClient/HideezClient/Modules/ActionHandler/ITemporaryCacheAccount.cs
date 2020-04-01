using HideezClient.Models;
using HideezClient.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezClient.Modules.ActionHandler
{
    interface ITemporaryCacheAccount
    {
        TemporaryCache<AccountModel> OtpReqCache { get; set; }
        TemporaryCache<AccountModel> PasswordReqCache { get; set; }

        void Clear();
    }
}
