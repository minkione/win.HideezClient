using Hideez.SDK.Communication.Log;
using HideezMiddleware;

namespace HideezClient.Modules.Log
{
    class LogManager
    {                         
        public static Logger GetCurrentClassLogger(string className)
        {
            return new Logger(className, new NLogWrapper());
        }

        public static void EnableLogging()
        {
            NLog.LogManager.EnableLogging();
        }

        public static void Flush()
        {
            NLog.LogManager.Flush();
        }

        public static void Shutdown()
        {
            NLog.LogManager.Shutdown();
        }

    }
}
