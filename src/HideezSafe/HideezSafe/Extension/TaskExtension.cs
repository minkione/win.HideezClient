using NLog;
using System.Threading.Tasks;

namespace HideezSafe.Extension
{
    public static class TaskExtension
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static void ForgetSafely(this Task task)
        {
            task.ContinueWith(HandleException);
        }

        private static void HandleException(Task task)
        {
            if (task.IsFaulted)
            {
                log.Error(task.Exception);
            }
        }
    }
}
