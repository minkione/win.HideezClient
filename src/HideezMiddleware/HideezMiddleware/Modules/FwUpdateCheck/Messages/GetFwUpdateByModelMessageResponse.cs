using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.FwUpdateCheck.Messages
{
    class GetFwUpdateByModelMessageResponse: PubSubMessageBase
    {
        public string FilePath { get; }

        public GetFwUpdateByModelMessageResponse(string filePath)
        {
            FilePath = filePath;
        }
    }
}
