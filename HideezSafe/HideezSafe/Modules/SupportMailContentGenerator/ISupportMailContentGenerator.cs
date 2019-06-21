using System.Threading.Tasks;

namespace HideezSafe.Modules
{
    interface ISupportMailContentGenerator
    {
        Task<string> GenerateSupportMail(string address);
    }
}
