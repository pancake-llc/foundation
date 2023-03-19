using System.Collections.Generic;
using Pancake.Attribute;
using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using System.Globalization;
using System.IO;
#endif

namespace Pancake.IAP
{
    [DeclareHorizontalGroup("button")]
    [HideMono]
    public class IAPSettings : ScriptableSettings<IAPSettings>
    {
        [InfoBox("Product id should look like : com.appname.itemid\nEx: com.eldenring.doublesoul\n\nConsumable        : purchase multiple time\nNon Consumable : purchase once time")]
        [InfoBox("When test mode is enabled all products will be initialized as Consumable (Android Only)")] [SerializeField]
        private bool testMode;

        [SerializeField] private List<IAPData> skusData = new List<IAPData>();
        
        public static bool TestMode => Instance.testMode;

        public static List<IAPData> SkusData => Instance.skusData;

        /// <summary>
        /// IAP 4.5.2
        /// </summary>
#if UNITY_EDITOR
        private string m_GoogleError;

        private string m_AppleError;
        [Space(20)] [SerializeField, TextArea] private string googlePlayStorePublicKey;

        [Group("button"), Button("Obfuscate Key")]
        private void ValidateObfuscator()
        {
            ObfuscateSecrets(true);

            var asmdef = (TextAsset) Resources.Load("iapGeneratedAsmdef", typeof(TextAsset));
            var meta = (TextAsset) Resources.Load("iapGeneratedAsmdefMeta", typeof(TextAsset));

            string path = Path.Combine(TangleFileConsts.k_OutputPath, "pancake@iap.generated.asmdef");
            string pathMeta = Path.Combine(TangleFileConsts.k_OutputPath, "pancake@iap.generated.asmdef.meta");
            if (!File.Exists(path))
            {
                var writer = new StreamWriter(path, false);
                writer.Write(asmdef.text);
                writer.Close();
            }

            if (!File.Exists(pathMeta))
            {
                var writer = new StreamWriter(pathMeta, false);
                writer.Write(meta.text);
                writer.Close();
            }

            AssetDatabase.ImportAsset(path);
        }

        [Group("button"), Button("Generate Product")]
        private void GenerateImplProduct()
        {
            const string path = "Assets/_Root/Scripts";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var productImplPath = $"{path}/Product.cs";

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

        private class ObfuscationGenerator
        {
            private const string m_GeneratedCredentialsTemplateFilename = "IAPGeneratedCredentials.cs.template";
            private const string m_GeneratedCredentialsTemplateFilenameNoExtension = "IAPGeneratedCredentials.cs";

            private const string k_AppleCertPath = "Packages/com.unity.purchasing/Editor/AppleIncRootCertificate.cer";
            private const string k_AppleStoreKitTestCertPath = "Packages/com.unity.purchasing/Editor/StoreKitTestCertificate.cer";

            private const string k_AppleClassIncompleteErr = "Invalid Apple Root Certificate";
            private const string k_AppleStoreKitTestClassIncompleteErr = "Invalid Apple StoreKit Test Certificate";

            internal static string ObfuscateAppleSecrets()
            {
                var appleError = WriteObfuscatedAppleClassAsAsset();

                AssetDatabase.Refresh();

                return appleError;
            }

            internal static string ObfuscateGoogleSecrets(string googlePlayPublicKey)
            {
                var googleError = WriteObfuscatedGooglePlayClassAsAsset(googlePlayPublicKey);

                AssetDatabase.Refresh();

                return googleError;
            }

            /// <summary>
            /// Generates specified obfuscated class files.
            /// </summary>
            internal static void ObfuscateSecrets(bool includeGoogle, ref string appleError, ref string googleError, string googlePlayPublicKey)
            {
                try
                {
                    // First things first! Obfuscate! XHTLOA!
                    appleError = WriteObfuscatedAppleClassAsAsset();

                    if (includeGoogle)
                    {
                        googleError = WriteObfuscatedGooglePlayClassAsAsset(googlePlayPublicKey);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.StackTrace);
                }

                // Ensure all the Tangle classes exist, even if they were not generated at this time.
                if (!DoesGooglePlayTangleClassExist())
                {
                    try
                    {
                        WriteObfuscatedClassAsAsset(TangleFileConsts.k_GooglePlayClassPrefix,
                            0,
                            new int[0],
                            new byte[0],
                            false);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(e.StackTrace);
                    }
                }

                AssetDatabase.Refresh();
            }

            private static string WriteObfuscatedAppleClassAsAsset()
            {
                var err = WriteObfuscatedAppleClassAsAsset(k_AppleCertPath, k_AppleClassIncompleteErr, TangleFileConsts.k_AppleClassPrefix);

                if (err == null)
                {
                    err = WriteObfuscatedAppleClassAsAsset(k_AppleStoreKitTestCertPath,
                        k_AppleStoreKitTestClassIncompleteErr,
                        TangleFileConsts.k_AppleStoreKitTestClassPrefix);
                }

                return err;
            }

            private static string WriteObfuscatedAppleClassAsAsset(string certPath, string classIncompleteErr, string classPrefix)
            {
                string appleError = null;
                var key = 0;
                var order = new int[0];
                var tangled = new byte[0];
                try
                {
                    var bytes = File.ReadAllBytes(certPath);
                    order = new int[bytes.Length / 20 + 1];

                    // TODO: Integrate with upgraded Tangle!

                    tangled = TangleObfuscator.Obfuscate(bytes, order, out key);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{classIncompleteErr}. Generating incomplete credentials file. " + e);
                    appleError = $"  {classIncompleteErr}";
                }

                WriteObfuscatedClassAsAsset(classPrefix,
                    key,
                    order,
                    tangled,
                    tangled.Length != 0);

                return appleError;
            }

            private static string WriteObfuscatedGooglePlayClassAsAsset(string googlePlayPublicKey)
            {
                string googleError = null;
                var key = 0;
                var order = new int[0];
                var tangled = new byte[0];
                try
                {
                    var bytes = Convert.FromBase64String(googlePlayPublicKey);
                    order = new int[bytes.Length / 20 + 1];

                    tangled = TangleObfuscator.Obfuscate(bytes, order, out key);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Invalid Google Play Public Key. Generating incomplete credentials file. " + e);
                    googleError = "  The Google Play License Key is invalid. GooglePlayTangle was generated with incomplete credentials.";
                }

                WriteObfuscatedClassAsAsset(TangleFileConsts.k_GooglePlayClassPrefix,
                    key,
                    order,
                    tangled,
                    tangled.Length != 0);

                return googleError;
            }

            private static string FullPathForTangleClass(string classnamePrefix)
            {
                return Path.Combine(TangleFileConsts.k_OutputPath, string.Format($"{classnamePrefix}{TangleFileConsts.k_ObfuscationClassSuffix}"));
            }

            internal static bool DoesAppleTangleClassExist()
            {
                return ObfuscatedClassExists(TangleFileConsts.k_AppleClassPrefix) && ObfuscatedClassExists(TangleFileConsts.k_AppleStoreKitTestClassPrefix);
            }

            internal static bool DoesGooglePlayTangleClassExist() { return ObfuscatedClassExists(TangleFileConsts.k_GooglePlayClassPrefix); }

            private static bool ObfuscatedClassExists(string classnamePrefix) { return File.Exists(FullPathForTangleClass(classnamePrefix)); }

            private static void WriteObfuscatedClassAsAsset(string classnamePrefix, int key, int[] order, byte[] data, bool populated)
            {
                var substitutionDictionary = new Dictionary<string, string>()
                {
                    {"{NAME}", classnamePrefix.ToString()},
                    {"{KEY}", key.ToString()},
                    {"{ORDER}", String.Format("{0}", String.Join(",", Array.ConvertAll(order, i => i.ToString())))},
                    {"{DATA}", Convert.ToBase64String(data)},
                    {"{POPULATED}", populated.ToString().ToLowerInvariant()} // Defaults to XML-friendly values
                };

                var templateText = LoadTemplateText(out var templateRelativePath);

                if (templateText != null)
                {
                    var outfileText = templateText;

                    // Apply the parameters to the template
                    foreach (var pair in substitutionDictionary)
                    {
                        outfileText = outfileText.Replace(pair.Key, pair.Value);
                    }

                    Directory.CreateDirectory(TangleFileConsts.k_OutputPath);
                    File.WriteAllText(FullPathForTangleClass(classnamePrefix), outfileText);
                }
            }

            /// <summary>
            /// Loads the template file.
            /// </summary>
            /// <returns>The template file's text.</returns>
            /// <param name="templateRelativePath">Relative Assets/ path to template file.</param>
            private static string LoadTemplateText(out string templateRelativePath)
            {
                var assetGUIDs = AssetDatabase.FindAssets(m_GeneratedCredentialsTemplateFilenameNoExtension);
                string templateGUID = null;
                templateRelativePath = null;

                if (assetGUIDs.Length > 0)
                {
                    templateGUID = assetGUIDs[0];
                }
                else
                {
                    Debug.LogError(string.Format("Could not find template \"{0}\"", m_GeneratedCredentialsTemplateFilename));
                }

                string templateText = null;

                if (templateGUID != null)
                {
                    templateRelativePath = AssetDatabase.GUIDToAssetPath(templateGUID);

                    var templateAbsolutePath = Path.GetDirectoryName(Application.dataPath) + Path.DirectorySeparatorChar + templateRelativePath;

                    templateText = File.ReadAllText(templateAbsolutePath);
                }

                return templateText;
            }
        }

        private class TangleFileConsts
        {
            internal const string k_OutputPath = "Assets/_Root/Scripts/UnityPurchasing/generated";

            internal const string k_AppleClassPrefix = "Apple";
            internal const string k_AppleStoreKitTestClassPrefix = "AppleStoreKitTest";
            internal const string k_GooglePlayClassPrefix = "GooglePlay";
            internal const string k_ObfuscationClassSuffix = "Tangle.cs";
        }

        /// <summary>
        /// This class will generate the tangled signature used for client-side receipt validation obfuscation.
        /// </summary>
        public static class TangleObfuscator
        {
            /// <summary>
            /// An Exception thrown when the tangle order array provided is invalid or shorter than the number of data slices made.
            /// </summary>
            public class InvalidOrderArray : Exception
            {
            }

            /// <summary>
            /// Generates the obfucscation tangle data.
            /// </summary>
            /// <param name="data"> The Apple or GooglePlay public key data to be obfuscated. </param>
            /// <param name="order"> The array, passed by reference, of order of the data slices used to obfuscate the data with. </param>
            /// <param name="rkey"> Outputs the encryption key to deobfuscate the tangled data at runtime </param>
            /// <returns>The obfucated public key</returns>
            public static byte[] Obfuscate(byte[] data, int[] order, out int rkey)
            {
                var rnd = new System.Random();
                var key = rnd.Next(2, 255);
                var res = new byte[data.Length];
                var slices = data.Length / 20 + 1;

                if (order == null || order.Length < slices)
                {
                    throw new InvalidOrderArray();
                }

                Array.Copy(data, res, data.Length);
                for (var i = 0; i < slices - 1; i++)
                {
                    var j = rnd.Next(i, slices - 1);
                    order[i] = j;
                    var sliceSize = 20; // prob should be configurable
                    var tmp = res.Skip(i * 20).Take(sliceSize).ToArray(); // tmp = res[i*20 .. slice]
                    Array.Copy(res,
                        j * 20,
                        res,
                        i * 20,
                        sliceSize); // res[i] = res[j*20 .. slice]
                    Array.Copy(tmp,
                        0,
                        res,
                        j * 20,
                        sliceSize); // res[j] = tmp
                }

                order[slices - 1] = slices - 1;

                rkey = key;
                return res.Select(x => (byte) (x ^ key)).ToArray();
            }
        }

        private void ObfuscateSecrets(bool includeGoogle)
        {
            ObfuscationGenerator.ObfuscateSecrets(includeGoogle: includeGoogle,
                appleError: ref m_AppleError,
                googleError: ref m_GoogleError,
                googlePlayPublicKey: googlePlayStorePublicKey);
        }
#endif
    }
}