#if PANCAKE_PLAYFAB
using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Pancake.GameService
{
    /// <summary>
    /// bridge setting for PlayfabSharedSettings
    /// </summary>
    public class ServiceSettings : ScriptableObject
    {
        public static int delayFetchRank = 180;
        public const string INTERNAL_CONFIG_KEY = "__internal_config";
        internal static ServiceSettings instance;
        internal static PlayFabSharedSettings sharedSettings;

        public static ServiceSettings Instance
        {
            get
            {
                if (instance != null) return instance;

                instance = LoadSettings();
                if (instance == null)
                {
#if UNITY_EDITOR
                    CreateSettingsAsset();
                    instance = LoadSettings();
#else
                    instance = UnityEngine.ScriptableObject.CreateInstance<Pancake.GameService.ServiceSettings>();
                    Debug.LogWarning("ServiceSettings not found! Please go to menu Tools > Pancake > Playfab to setup and build again!");
#endif
                }

                return instance;
            }
        }

        public static PlayFabSharedSettings SharedSettings
        {
            get
            {
                if (sharedSettings != null) return sharedSettings;

                sharedSettings = LoadPlayFabSharedSettings();
                if (sharedSettings == null)
                {
#if UNITY_EDITOR
                    CreatePlayFabSharedSettings();
                    sharedSettings = LoadPlayFabSharedSettings();
#else
                    sharedSettings = UnityEngine.ScriptableObject.CreateInstance<PlayFabSharedSettings>();
                    Debug.LogWarning("PlayFabSharedSettings not found! Please go to menu Tools > Pancake > Playfab to setup and build again!");
#endif
                }

                return sharedSettings;
            }
        }

        public static PlayFabSharedSettings LoadPlayFabSharedSettings() { return Resources.Load<PlayFabSharedSettings>("PlayFabSharedSettings"); }

        [SerializeField] private string titleId;
        [SerializeField] private string secretKey;
        [SerializeField] private WebRequestType requestType = WebRequestType.UnityWebRequest;

        [SerializeField] private bool useCustomIdAsDefault = true;
        [SerializeField] private bool enableAdminApi = true;
        [SerializeField] private bool enableClientApi = true;
        [SerializeField] private bool enableEntityApi = true;
        [SerializeField] private bool enableServerApi = true;
        [SerializeField] private bool enableRequestTimesApi;
        [SerializeField] private GetPlayerCombinedInfoRequestParams infoRequestParams;

        private InternalConfig _internalConfig;
        private Func<string> _funcField1;
        private Func<string> _funcField2;
        private Func<string> _funcField3;
        private Func<string> _funcField4;
        private Func<string> _funcField5;
        public static bool EnableAdminApi => Instance.enableAdminApi;
        public static bool EnableClientApi => Instance.enableClientApi;
        public static bool EnableEntityApi => Instance.enableEntityApi;
        public static bool EnableServerApi => Instance.enableServerApi;
        public static bool EnableRequestTimesApi => Instance.enableRequestTimesApi;

        public static string TitleId => Instance.titleId;

        public static string SecretKey => Instance.secretKey;

        public static WebRequestType RequestType => Instance.requestType;

        public static GetPlayerCombinedInfoRequestParams InfoRequestParams => Instance?.infoRequestParams;

        public static bool UseCustomIdAsDefault => Instance.useCustomIdAsDefault;

        public static InternalConfig InternalConfig => Instance._internalConfig;

        public static ServiceSettings LoadSettings() { return Resources.Load<ServiceSettings>("GameServiceSettings"); }

#if UNITY_EDITOR

        internal static bool flagCreateServiceSetting = false;
        internal static void CreateSettingsAsset()
        {
            if (flagCreateServiceSetting) return;
            flagCreateServiceSetting = true;
            
            // Now create the asset inside the Resources folder.
            var setting = UnityEngine.ScriptableObject.CreateInstance<Pancake.GameService.ServiceSettings>();
            UnityEditor.AssetDatabase.CreateAsset(setting, $"{DefaultResourcesPath()}/GameServiceSettings.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"ServiceSettings was created at {DefaultResourcesPath()}/GameServiceSettings.asset");
        }

        internal static void CreatePlayFabSharedSettings()
        {
            // Now create the asset inside the Resources folder.
            var setting = UnityEngine.ScriptableObject.CreateInstance<PlayFabSharedSettings>();
            UnityEditor.AssetDatabase.CreateAsset(setting, $"{DefaultResourcesPath()}/PlayFabSharedSettings.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"PlayFabSharedSettings was created at {DefaultResourcesPath()}/PlayFabSharedSettings.asset");
        }

        private static string DefaultResourcesPath()
        {
            const string defaultResourcePath = "Assets/_Root/Resources";
            if (!defaultResourcePath.DirectoryExists()) defaultResourcePath.CreateDirectory();
            return defaultResourcePath;
        }
#endif


        private void OnEnable()
        {
            if (InfoRequestParams != null && !InfoRequestParams.UserDataKeys.Exists(_ => _.Equals(INTERNAL_CONFIG_KEY)))
            {
                InfoRequestParams.UserDataKeys.Add(INTERNAL_CONFIG_KEY);
            }
        }

        /// <summary>
        /// use for setup way get value for internal config
        /// </summary>
        /// <param name="funcField1"></param>
        /// <param name="funcField2"></param>
        /// <param name="funcField3"></param>
        /// <param name="funcField4"></param>
        /// <param name="funcField5"></param>
        public static void Register(
            Func<string> funcField1 = null,
            Func<string> funcField2 = null,
            Func<string> funcField3 = null,
            Func<string> funcField4 = null,
            Func<string> funcField5 = null)
        {
            if (funcField1 != null) Instance._funcField1 = funcField1;
            if (funcField2 != null) Instance._funcField2 = funcField2;
            if (funcField3 != null) Instance._funcField3 = funcField3;
            if (funcField4 != null) Instance._funcField4 = funcField4;
            if (funcField5 != null) Instance._funcField5 = funcField5;
        }

        public static InternalConfig Get(string countryCode)
        {
            if (InternalConfig == null) Instance._internalConfig = new InternalConfig();

            InternalConfig.countryCode = countryCode;
            InternalConfig.field1 = Instance._funcField1?.Invoke(); // if Instance._funcField1 null => InternalConfig.field1 = null
            InternalConfig.field2 = Instance._funcField2?.Invoke();
            InternalConfig.field3 = Instance._funcField3?.Invoke();
            InternalConfig.field4 = Instance._funcField4?.Invoke();
            InternalConfig.field5 = Instance._funcField5?.Invoke();
            return InternalConfig;
        }
    }
}
#endif