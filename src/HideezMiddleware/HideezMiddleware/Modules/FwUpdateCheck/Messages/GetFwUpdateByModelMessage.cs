using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.FwUpdateCheck.Messages
{
    class GetFwUpdateByModelMessage: PubSubMessageBase
    {
        public string DeviceModel { get; }
        public GetFwUpdateByModelMessage(string deviceModel)
        {
            DeviceModel = deviceModel;
        }
    }
}
