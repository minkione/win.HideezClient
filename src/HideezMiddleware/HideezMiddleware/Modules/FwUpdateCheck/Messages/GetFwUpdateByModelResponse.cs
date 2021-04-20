using Meta.Lib.Modules.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HideezMiddleware.Modules.FwUpdateCheck.Messages
{
    public class GetFwUpdateByModelResponse: PubSubMessageBase
    {
        public string FilePath { get; }

        public GetFwUpdateByModelResponse(string filePath)
        {
            FilePath = filePath;
        }
    }
}
