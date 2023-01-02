#if PANCAKE_IAP
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pancake.IAP
{
    public class IAPSettings : ScriptableSettings<IAPSettings>
    {
        #region member

        [InfoBox("Product id should look like : com.appname.itemid\ncom.eldenring.doublesoul\n\nConsumable: purchase multiple time\nNon Consumable: purchase once time")]
        [SerializeField]
        private bool runtimeAutoInitialize = true;

        [InfoBox("When test mode is enabled all products will be initialized as Consumable (Android Only)")] [SerializeField]
        private bool testMode = false;

        [SerializeField] private List<IAPData> skusData = new List<IAPData>();

        #endregion

        #region properties

        public static bool RuntimeAutoInitialize => Instance.runtimeAutoInitialize;

        public static bool TestMode => Instance.testMode;

        public static List<IAPData> SkusData => Instance.skusData;

        #endregion


#if UNITY_EDITOR
        [Button("Generate")]
        private void GenerateImplProduct()
        {
            const string path = "Assets/_Root/Scripts";
            var productImplPath = $"{path}/Product.cs";
            if (!path.DirectoryExists()) path.CreateDirectory();

            var str = "namespace Pancake.IAP\n{";
            str += "\n\tpublic static class Product\n\t{";

            var skus = SkusData;
            for (int i = 0; i < skus.Count; i++)
            {
                var itemName = skus[i].sku.Id.Split('.').Last();
                str += $"\n\t\tpublic static IAPData Purchase{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemName)}()";
                str += "\n\t\t{";
                str += $"\n\t\t\treturn IAPManager.Purchase(IAPSettings.SkusData[{i}]);";
                str += "\n\t\t}";
                str += "\n";
                
                str += $"\n\t\tpublic static bool IsPurchased{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemName)}()";
                str += "\n\t\t{";
                str += $"\n\t\t\treturn IAPManager.Instance.IsPurchased(IAPSettings.SkusData[{i}].sku.Id);";
                str += "\n\t\t}";
                str += "\n";
            }

            str += "\n\t}";
            str += "\n}";

            var writer = new StreamWriter(productImplPath, false);
            writer.Write(str);
            writer.Close();
            AssetDatabase.ImportAsset(productImplPath);
        }

#endif
    }
}
#endif