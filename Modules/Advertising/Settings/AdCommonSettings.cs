using System;
using System.IO;
using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    [DeclareBoxGroup("basic", Title = "Basic")]
    [DeclareBoxGroup("load-setting", Title = "Load Setting")]
    public class AdCommonSettings
    {
        [SerializeField, Group("basic"), LabelText("Auto Initialize*"),
         Tooltip("When AutoInitialize is enabled it will automatically create a gameObject in Runtime containing the component Advertising and init it!")]
        private bool autoInit = true;

        [SerializeField, Group("load-setting"), LabelText("Type")] private EAutoLoadingAd autoLoadingAd = EAutoLoadingAd.All;

        [Range(5, 100), SerializeField, Group("load-setting"), LabelText("Checking Interval*"), Tooltip("Time interval between two checks in seconds")]
        private float adCheckingInterval = 8f;

        [Range(5, 100), SerializeField, Group("load-setting"), LabelText("Loading Interval*"), Tooltip("Time interval between two loads in seconds")]
        private float adLoadingInterval = 15f;

        [SerializeField, Group("basic"), LabelText("GDPR")] private bool enableGdpr;

        [SerializeField, Group("basic"), ShowIf(nameof(enableGdpr)), Indent]
        private string privacyUrl;

        [SerializeField, Group("basic"), ValidateInput(nameof(MultidexValidate)), LabelText("MultiDex*"), Tooltip("Android only")]
        private bool multiDex;

        [SerializeField, Group("basic"), LabelText("Hide AppOpenAd*"), Tooltip("Hide App Open Ad When Starup")]
        private bool hideAppOpenAdStartup;

        [SerializeField, Group("basic")] private EAdNetwork currentNetwork = EAdNetwork.Applovin;

        public bool AutoInit => autoInit;
        public float AdCheckingInterval => adCheckingInterval;
        public float AdLoadingInterval => adLoadingInterval;
        public string PrivacyUrl => privacyUrl;
        public bool EnableGdpr => enableGdpr;
        public bool MultiDex => multiDex;
        public bool HideAppOpenAdWhenStartup => hideAppOpenAdStartup;

        public EAdNetwork CurrentNetwork { get => currentNetwork; set => currentNetwork = value; }
        public EAutoLoadingAd AutoLoadingAd { get => autoLoadingAd; set => autoLoadingAd = value; }


        private ValidationResult MultidexValidate()
        {
#if UNITY_EDITOR
            const string androidPath = "Assets/Plugins/Android/";
            const string mainTemplatePath = "Assets/Plugins/Android/mainTemplate.gradle";
            const string gradleTemplatePath = "Assets/Plugins/Android/gradleTemplate.properties";
            if (multiDex)
            {
                if (!Directory.Exists(androidPath)) Directory.CreateDirectory(androidPath);
                if (File.Exists(mainTemplatePath)) return ValidationResult.Valid;
                var upmPath = $"Packages/com.pancake.heart/Editor/Misc/Templates/mainTemplate.txt";
                var normalPath = $"Assets/heart/Editor/Misc/Templates/mainTemplate.txt";
                string path = !File.Exists(Path.GetFullPath(upmPath)) ? normalPath : upmPath;
                string mainTemplate = (UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset)) as TextAsset)?.text;
                var writer = new StreamWriter(mainTemplatePath, false);
                writer.Write(mainTemplate);
                writer.Close();
                UnityEditor.AssetDatabase.ImportAsset(mainTemplatePath);
            }
            else
            {
                if (!File.Exists(mainTemplatePath)) return ValidationResult.Valid;
                UnityEditor.FileUtil.DeleteFileOrDirectory(mainTemplatePath);
                UnityEditor.FileUtil.DeleteFileOrDirectory(mainTemplatePath + ".meta");
                UnityEditor.AssetDatabase.Refresh();
            }
#endif

            return ValidationResult.Valid;
        }
    }
}