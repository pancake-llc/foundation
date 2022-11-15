using UnityEngine;

namespace Pancake.Monetization
{
    public class Settings : ScriptableObject
    {
        private static Settings instance;

        public static Settings Instance
        {
            get
            {
                if (instance != null) return instance;

                instance = LoadSetting();

                if (instance == null)
                {
#if UNITY_EDITOR
                    CreateSettingsAsset();
                    instance = LoadSetting();
#else
                    instance = UnityEngine.ScriptableObject.CreateInstance<Pancake.Monetization.Settings>();
                    Debug.LogWarning("AdSettings not found! Please go to menu Tools > Pancake > Advertisement to setup and build again!");
#endif
                }

                return instance;
            }
        }

        #region member

        [SerializeField] private bool runtimeAutoInitialize = true;
        [SerializeField] private AdSettings adSettings = new AdSettings();
        [SerializeField] private AdmobSettings admobSettings = new AdmobSettings();
        [SerializeField] private MaxSettings maxSettings = new MaxSettings();
        [SerializeField] private IronSourceSettings ironSourceSettings = new IronSourceSettings();

        #endregion

        #region properties

        public static bool RuntimeAutoInitialize => Instance.runtimeAutoInitialize;

        public static AdSettings AdSettings => Instance.adSettings;

        public static AdmobSettings AdmobSettings => Instance.admobSettings;

        public static MaxSettings MaxSettings => Instance.maxSettings;
        public static IronSourceSettings IronSourceSettings => Instance.ironSourceSettings;

        public static EAdNetwork CurrentNetwork => Instance.adSettings.CurrentNetwork;

        #endregion

        #region api

        public static Settings LoadSetting() { return Resources.Load<Monetization.Settings>("AdSettings"); }

#if UNITY_EDITOR
        private static bool flagCreateSetting = false;
        internal static void CreateSettingsAsset()
        {
            if (flagCreateSetting) return;
            flagCreateSetting = true;

            // Now create the asset inside the Resources folder.
            var setting = UnityEngine.ScriptableObject.CreateInstance<Pancake.Monetization.Settings>();
            UnityEditor.AssetDatabase.CreateAsset(setting, $"{DefaultResourcesPath()}/AdSettings.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"AdSettings was created at {DefaultResourcesPath()}/AdSettings.asset");
        }
        
        private static string DefaultResourcesPath()
        {
            const string defaultResourcePath = "Assets/_Root/Resources";
            if (!defaultResourcePath.DirectoryExists()) defaultResourcePath.CreateDirectory();
            return defaultResourcePath;
        }
#endif

        #endregion
    }
}