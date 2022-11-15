using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Monetization
{
    public static class RuntimeManager
    {
        public static readonly DateTime UnixEpoch = DateTime.SpecifyKind(new DateTime(1970, 1, 1), DateTimeKind.Utc);
        private const string APP_INSTALLATION_TIMESTAMP_PPKEY = "APP_INSTALLATION_TIMESTAMP";

        private static bool mIsInitialized;

        #region Public API

      
        public static event Action Initialized;

        /// <summary>
        /// Initializes the runtime. Always do this before
        /// accessing API. It's recommended to initialize as 
        /// early as possible, ideally as soon as the app launches. This
        /// method is a no-op if the runtime has been initialized before, so it's
        /// safe to be called multiple times. This method must be called on 
        /// the main thread.
        /// </summary>
        public static void Init()
        {
            if (mIsInitialized)
            {
                return;
            }

            if (Application.isPlaying)
            {
                // Initialize runtime Helper.
                RuntimeHelper.Init();
                
                var go = new GameObject("RuntimeManager");
                Configure(go);

                // Store the timestamp of the *first* init which can be used 
                // as a rough approximation of the installation time.
                if (StorageUtil.GetTime(APP_INSTALLATION_TIMESTAMP_PPKEY, UnixEpoch) == UnixEpoch) StorageUtil.SetTime(APP_INSTALLATION_TIMESTAMP_PPKEY, DateTime.Now);

                // Done init.
                mIsInitialized = true;

                // Raise the event.
                Initialized?.Invoke();

                Debug.Log("RuntimeManager has been initialized.");
            }
        }
        
        public static bool IsInitialized() { return mIsInitialized; }

        /// <summary>
        /// Gets the installation timestamp of this app in local timezone.
        /// This timestamp is recorded when the app is initialized for
        /// the first time so it's not really precise but can serve well as a rough approximation
        /// provided that the initialization is done soon after app launch.
        /// </summary>
        /// <returns>The installation timestamp.</returns>
        public static DateTime GetAppInstallationTimestamp() { return StorageUtil.GetTime(APP_INSTALLATION_TIMESTAMP_PPKEY, UnixEpoch); }

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
        private static void AutoInitialize()
        {
            if (Settings.RuntimeAutoInitialize) Init();
        }

        // Adds the required components necessary for the runtime operation of EM modules
        // to the game object this instance is attached to.
        private static void Configure(GameObject go)
        {
            // This game object must prevail.
            // ReSharper disable once AccessToStaticMemberViaDerivedType
            GameObject.DontDestroyOnLoad(go);
            go.AddComponent<Advertising>();
#if PANCAKE_IRONSOURCE_ENABLE
            go.AddComponent<IronSourceStateHandler>();
#endif
        }

        #endregion
    }
}