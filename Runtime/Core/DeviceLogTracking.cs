using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Pancake
{
    public static class DeviceLogTracking
    {
        private static bool isInitialized;
        public static StringBuilder sessionLogError;

        internal static void Init()
        {
            if (isInitialized) return;
            isInitialized = true;
            // tracking log
            sessionLogError = new StringBuilder();
            Application.logMessageReceived -= OnHandleLogReceived;
            Application.logMessageReceived += OnHandleLogReceived;
            RuntimeManager.AddQuitCallback(OnApplicationQuit);
        }

        /// <summary>
        /// handle when receive log
        /// </summary>
        /// <param name="log"></param>
        /// <param name="stacktrace"></param>
        /// <param name="type"></param>
        private static void OnHandleLogReceived(string log, string stacktrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                sessionLogError.AppendLine(log);
                sessionLogError.AppendLine(stacktrace);
            }
        }

        private static void OnApplicationQuit()
        {
            // remove old log outdate
            RemoveOldLogDirectory(3);

            // write current log
            WriteLocalLog();
        }

        /// <summary>
        /// write log to local file
        /// </summary>
        /// <returns></returns>
        private static string WriteLocalLog()
        {
            var log = sessionLogError.ToString();
            if (!string.IsNullOrEmpty(log))
            {
                // create the directory
                var feedbackDirectory = $"{Application.persistentDataPath}/userlogs/{DateTime.Now:ddMMyyyy}/{DateTime.Now:HHmmss}";
                if (!feedbackDirectory.DirectoryExists()) feedbackDirectory.CreateDirectory();

                // save the log
                File.WriteAllText(feedbackDirectory + "/logs.txt", log);

                return feedbackDirectory;
            }

            return "";
        }

        /// <summary>
        /// Removes stale logs that exceed the number of days specified by <paramref name="day"/>
        /// </summary>
        /// <param name="day"></param>
        private static void RemoveOldLogDirectory(int day)
        {
            var path = $"{Application.persistentDataPath}/userlogs";
            if (path.DirectoryExists())
            {
                DirectoryInfo info = new DirectoryInfo(path);
                foreach (var directoryInfo in info.GetDirectories())
                {
                    string folderName = directoryInfo.Name;
                    DateTime.TryParseExact(folderName,
                        "ddMMyyyy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var dateTime);

                    if ((DateTime.Now - dateTime).TotalDays >= day) $"{path}/{folderName}".DeleteDirectory();
                }
            }
        }
    }
}