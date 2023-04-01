using System.Collections.Generic;
using Pancake;
using Pancake.Attribute;
using Unity.Android.Types;
using UnityEditor;
using UnityEngine;
using AndroidArchitecture = UnityEditor.AndroidArchitecture;

namespace PancakeEditor
{
    [HideMono]
    [EditorIcon("scriptable_build")]
    public class EditorPreBuildSettings : ScriptableSettings<EditorPreBuildSettings>
    {
        public string companyName;
        public string productName;
        public string packageName = "com.company.product";
        public string version = "1.0.0";
        public int versionCode = 1;
#if UNITY_ANDROID
        public EScriptingBackend scriptingBackend = EScriptingBackend.IL2CPP;
        public EAndroidArchitecture architecture = EAndroidArchitecture.ARMv7_ARM64;
        public bool appBundle = false;
        [Space] public string pathKeystore;
        public string keystorePass;
        public string keyaliasName;
        public string keyaliasPass;
        [Space] public string outputFolder;
        public static bool AppBundle { get => Instance.appBundle; set => Instance.appBundle = value; }
        public static string OutputFolder => Instance.outputFolder;
        public static string PathKeystore { get => Instance.pathKeystore; set => Instance.pathKeystore = value; }
        public static string KeystorePass { get => Instance.keystorePass; set => Instance.keystorePass = value; }
        public static string KeyaliasName { get => Instance.keyaliasName; set => Instance.keyaliasName = value; }
        public static string KeyaliasPass { get => Instance.keyaliasPass; set => Instance.keyaliasPass = value; }
        public static EAndroidArchitecture Architecture { get => Instance.architecture; set => Instance.architecture = value; }
        public static EScriptingBackend ScriptingBackend { get => Instance.scriptingBackend; set => Instance.scriptingBackend = value; }
#endif
        [Space] [SerializeField, ReadOnly] private List<EditorPreBuildCondition> defaultConditions;
        [SerializeField] private List<EditorPreBuildCondition> extendConditions;

        public static string CompanyName { get => Instance.companyName; set => Instance.companyName = value; }
        public static string ProductName { get => Instance.productName; set => Instance.productName = value; }
        public static string PackageName { get => Instance.packageName; set => Instance.packageName = value; }
        public static string Version { get => Instance.version; set => Instance.version = value; }
        public static int VersionCode { get => Instance.versionCode; set => Instance.versionCode = value; }
        public static List<EditorPreBuildCondition> DefaultConditions => Instance.defaultConditions;
        public static List<EditorPreBuildCondition> ExtendConditions => Instance.extendConditions;

        public enum EScriptingBackend
        {
            Mono = 0,
            IL2CPP
        }

        public enum EAndroidArchitecture
        {
            ARMv7,
            ARM64,

            // ReSharper disable once UnusedMember.Global
            ARMv7_ARM64,
        }


        private void OnEnable()
        {
            defaultConditions = new List<EditorPreBuildCondition>
            {
                EditorResources.PreBuildConditionGlobalAsset,
                EditorResources.PreBuildConditionAdjustAsset,
                EditorResources.PreBuildConditionAdvertisingAsset,
                EditorResources.PreBuildConditionFirebaseAsset,
                EditorResources.PreBuildConditionNotificationAsset,
                EditorResources.PreBuildConditionPurchasingAsset
            };
        }
    }
}