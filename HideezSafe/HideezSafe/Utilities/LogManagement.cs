using NLog;
using NLog.Layouts;
using NLog.Targets;

namespace HideezSafe.Utilities
{
    static class LogManagement
    {
        private const string DefaultTargetName = "logfile";

        public static string GetTargetFilename(string targetName)
        {
            var target = GetTarget<FileTarget>(targetName);
            if (null == target) return null;

            var layout = target.FileName as SimpleLayout;
            if (null == layout) return null;

            // layout.Text provides the filename "template"
            // LogEventInfo is required; might make sense for a log line template but not really filename
            var filename = layout.Render(new LogEventInfo()).Replace(@"/", @"");
            return filename;
        }

        public static string GetTargetFilename()
        {
            return GetTargetFilename(DefaultTargetName);
        }

        public static string GetTargetFolder(string folder)
        {
            var layout = new SimpleLayout(folder);
            var path = layout.Render(new LogEventInfo()).Replace(@"/", @"");
            return path;
        }

        private static T GetTarget<T>(string targetName)
            where T : Target
        {
            if (null == LogManager.Configuration) return null;
            var target = LogManager.Configuration.FindTargetByName(targetName) as T;
            return target;
        }

    }
}
