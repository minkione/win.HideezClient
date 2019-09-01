using System.Threading.Tasks;

namespace HideezClient.Modules
{
    interface ISupportMailContentGenerator
    {
        Task<string> GenerateSupportMail(string address);
    }
}
