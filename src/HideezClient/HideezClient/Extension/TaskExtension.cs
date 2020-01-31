using Hideez.SDK.Communication.Log;
using HideezClient.Modules.Log;
using System.Threading.Tasks;

namespace HideezClient.Extension
{
    public static class TaskExtension
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger(nameof(TaskExtension));

        public static void ForgetSafely(this Task task)
        {
            task.ContinueWith(HandleException);
        }

        private static void HandleException(Task task)
        {
            if (task.IsFaulted)
            {
                log.WriteLine(task.Exception);
            }
        }
    }
}
