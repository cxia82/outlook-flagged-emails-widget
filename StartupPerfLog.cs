using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NotificationWidget
{
    internal static class StartupPerfLog
    {
        private const long MaxLogSizeBytes = 1024 * 1024;
        private static readonly Stopwatch AppStopwatch = Stopwatch.StartNew();
        private static readonly object Sync = new();
        private static readonly string LogPath = BuildLogPath();

        internal static void Write(string message)
        {
            try
            {
                var line = $"[{DateTime.Now:HH:mm:ss.fff}] +{AppStopwatch.ElapsedMilliseconds}ms {message}{Environment.NewLine}";

                lock (Sync)
                {
                    var directory = Path.GetDirectoryName(LogPath);
                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);

                    RotateLogIfNeeded();
                    File.AppendAllText(LogPath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // Intentionally ignore logging failures to avoid impacting startup.
            }
        }

        private static string BuildLogPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "NotificationWidget", "startup-perf.log");
        }

        private static void RotateLogIfNeeded()
        {
            if (!File.Exists(LogPath)) return;

            var logInfo = new FileInfo(LogPath);
            if (logInfo.Length < MaxLogSizeBytes) return;

            var archivedPath = LogPath + ".1";
            if (File.Exists(archivedPath))
                File.Delete(archivedPath);

            File.Move(LogPath, archivedPath);
        }
    }
}
