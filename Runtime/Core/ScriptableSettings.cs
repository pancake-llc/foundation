using UnityEngine;

namespace Pancake
{
    public abstract class ScriptableSettings<T> : ScriptableObject where T : ScriptableObject
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance != null) return instance;

                instance = LoadSettings();
                if (instance == null)
                {
#if UNITY_EDITOR
                    CreateSettingAssets();
                    instance = LoadSettings();
#else
                    instance = UnityEngine.ScriptableObject.CreateInstance<T>();
                    Debug.LogWarning($"{nameof(T)} not found! Please create instance to setup and build again!");
#endif
                }

                return instance;
            }
        }

        public static T LoadSettings()
        {
            return Resources.Load<T>(typeof(T).Name);
        }

#if UNITY_EDITOR
        // ReSharper disable once StaticMemberInGenericType
        // flagCreateSetting do not same value of each type
        private static bool flagCreateSetting;

        private static void CreateSettingAssets()
        {
            // if (flagCreateSetting) return;
            // flagCreateSetting = true;

            var setting = UnityEngine.ScriptableObject.CreateInstance<T>();
            UnityEditor.AssetDatabase.CreateAsset(setting, $"{DefaultResourcesPath()}/{typeof(T).Name}.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"{typeof(T).Name} was created ad {DefaultResourcesPath()}/{typeof(T).Name}.asset");
        }

        private static string DefaultResourcesPath()
        {
            const string defaultResourcePath = "Assets/_Root/Resources";
            if (!defaultResourcePath.DirectoryExists()) defaultResourcePath.CreateDirectory();
            return defaultResourcePath;
        }
#endif
    }
}