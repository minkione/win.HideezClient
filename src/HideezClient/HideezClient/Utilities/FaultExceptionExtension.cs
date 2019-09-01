using HideezClient.HideezServiceReference;
using System.ServiceModel;

namespace HideezClient
{
    static class FaultExceptionExtension
    {
        public static string FormattedMessage(this FaultException<HideezServiceFault> exc)
        {
            return $"{exc.Detail.FaultMessage} ({exc.Detail.ErrorCode})";
        }
    }
}
