using System;
using System.Globalization;
using System.Text;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake
{
    public static class RuntimeManager
    {
        public static readonly DateTime UnixEpoch = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
        private const string APP_INSTALLATION_TIMESTAMP_PPKEY = "APP_INSTALLATION_TIMESTAMP";

        private static bool isInitialized;

        public static StringBuilder sessionLogError;

        #region Public API

        public static event Action OnInitialized;

        /// <summary>
        /// Initializes the runtime. Always do this before
        /// accessing API. It's recommended to initialize as 
        /// early as possible, ideally as soon as the app launches. This
        /// method is a no-op if the runtime has been initialized before, so it's
        /// safe to be called multiple times. This method must be called on 
        /// the main thread.
        /// </summary>
        private static void Init()
        {
            if (isInitialized) return;

            if (Application.isPlaying)
            {
                // tracking log
                sessionLogError = new StringBuilder();
                Application.logMessageReceived -= OnHandleLogReceived;
                Application.logMessageReceived += OnHandleLogReceived;

                // Initialize runtime Helper.
                var runtimeHelper = new GameObject("RuntimeHelper") {hideFlags = HideFlags.HideInHierarchy};
                runtimeHelper.AddComponent<RuntimeHelper>();
                Object.DontDestroyOnLoad(runtimeHelper);
                RuntimeHelper.AddQuitCallback(OnApplicationQuit);

                if (Monetization.AdSettings.RuntimeAutoInitialize) AdConfigure(runtimeHelper);

                // Store the timestamp of the *first* init which can be used 
                // as a rough approximation of the installation time.
                if (StorageUtil.GetTime(APP_INSTALLATION_TIMESTAMP_PPKEY, UnixEpoch) == UnixEpoch) StorageUtil.SetTime(APP_INSTALLATION_TIMESTAMP_PPKEY, DateTime.Now);

                // Raise the event.
                OnInitialized?.Invoke();

                // Done init.
                isInitialized = true;

                Debug.Log("RuntimeManager has been initialized.");
            }
        }

        public static bool IsInitialized() { return isInitialized; }

        /// <summary>
        /// Gets the installation timestamp of this app in local timezone.
        /// This timestamp is recorded when the app is initialized for
        /// the first time so it's not really precise but can serve well as a rough approximation
        /// provided that the initialization is done soon after app launch.
        /// </summary>
        /// <returns>The installation timestamp.</returns>
        public static DateTime GetAppInstallationTimestamp => StorageUtil.GetTime(APP_INSTALLATION_TIMESTAMP_PPKEY, UnixEpoch);

        /// <summary>
        /// Enables or disables Unity debug log.
        /// </summary>
        /// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
        public static void EnableUnityDebugLog(bool isEnabled)
        {
#if UNITY_2017_1_OR_NEWER
            Debug.unityLogger.logEnabled = isEnabled;
#else
            Debug.logger.logEnabled = isEnabled;
#endif
        }

        #endregion

        #region Internal Stuff

        //Auto initialization
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize() { Init(); }

        // Adds the required components necessary for the runtime operation of Advertising
        // to the game object this instance is attached to.
        private static void AdConfigure(GameObject go)
        {
            // This game object must prevail.
            go.AddComponent<Monetization.Advertising>();
#if PANCAKE_IRONSOURCE_ENABLE
            go.AddComponent<Monetization.IronSourceStateHandler>();
#endif
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

        #endregion
    }
}