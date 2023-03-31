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
        public ScriptingBackend scriptingBackend = ScriptingBackend.IL2CPP;
        public AndroidArchitecture architecture = AndroidArchitecture.ARMv7_ARM64;
#endif
        [SerializeField, ReadOnly] private List<EditorPreBuildCondition> defaultConditions;
        [SerializeField] private List<EditorPreBuildCondition> extendConditions;

        public enum ScriptingBackend
        {
            Mono = 0,
            IL2CPP
        }
        public enum AndroidArchitecture
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