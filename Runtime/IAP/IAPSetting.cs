using System.Collections.Generic;
using UnityEngine;

namespace Pancake.IAP
{
    public class IAPSetting : ScriptableObject
    {
        private static IAPSetting instance;

        public static IAPSetting Instance
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
                    instance = UnityEngine.ScriptableObject.CreateInstance<Pancake.IAP.IAPSetting>();
                    Debug.LogWarning("IAPSetting not found! Please go to menu Tools > Pancake > IAP to setup and build again!");
#endif
                }

                return instance;
            }
        }

        #region member

        [InfoBox("Product id should look like : com.appname.itemid\ncom.eldenring.doublesoul\n\nConsumable: purchase multiple time\nNon Consumable: purchase once time")]
        [SerializeField]
        private bool runtimeAutoInitialize = true;

        [InfoBox("When test mode is enabled all products will be initialized as Consumable (Android Only)")]
        [SerializeField] private bool testMode = false;
        [SerializeField] private List<IAPData> skusData = new List<IAPData>();

        #endregion

        #region properties

        public static bool RuntimeAutoInitialize => Instance.runtimeAutoInitialize;

        public static bool TestMode => Instance.testMode;

        public static List<IAPData> SkusData => Instance.skusData;

        #endregion

        #region api

        public static IAPSetting LoadSettings() { return Resources.Load<IAPSetting>("IAPSettings"); }

#if UNITY_EDITOR
        private static bool flagCreateSetting = false;

        private static void CreateSettingsAsset()
        {
            if (flagCreateSetting) return;
            flagCreateSetting = true;

            // Now create the asset inside the Resources folder.
            var setting = UnityEngine.ScriptableObject.CreateInstance<Pancake.IAP.IAPSetting>();
            UnityEditor.AssetDatabase.CreateAsset(setting, $"{DefaultResourcesPath()}/IAPSettings.asset");
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"IAPSettings was created at {DefaultResourcesPath()}/IAPSettings.asset");
        }

        private static string DefaultResourcesPath()
        {
            const string defaultResourcePath = "Assets/_Root/Resources";
            if (!defaultResourcePath.DirectoryExists()) defaultResourcePath.CreateDirectory();
            return defaultResourcePath;
        }

        [Button("Generate")]
        private void GenerateImplProduct()
        {
            const string path = "Assets/_Root/Scripts";
            var productImplPath = $"{path}/Product.cs";
            if (!path.DirectoryExists()) path.CreateDirectory();

            var str = "namespace Pancake.IAP\n{";
            str += "\n\tpublic static class Product\n\t{";

            var skus = IAPSetting.SkusData;
            for (int i = 0; i < skus.Count; i++)
            {
                var itemName = skus[i].sku.Id.Split('.').Last();
                str += $"\n\t\tpublic static IAPData Purchase{System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemName)}()";
                str += "\n\t\t{";
                str += $"\n\t\t\treturn IAPManager.Purchase(IAPSetting.SkusData[{i}]);";
                str += "\n\t\t}";
                str += "\n";
            }

            str += "\n\t}";
            str += "\n}";

            var writer = new System.IO.StreamWriter(productImplPath, false);
            writer.Write(str);
            writer.Close();
            UnityEditor.AssetDatabase.ImportAsset(productImplPath);
        }

#endif

        #endregion
    }
}