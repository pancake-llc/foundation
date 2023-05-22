using System.Collections.Generic;
using Pancake.Apex;
using Pancake.ExLib;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace Pancake.Monetization
{
    [HideMonoScript]
    [EditorIcon("scriptable_ad")]
    public class AdSettings : ScriptableObject
    {
        [Message("Default, ad will be auto loading.")] [Range(5, 100), SerializeField]
        private float adCheckingInterval = 8f;

        [Range(5, 100), SerializeField] private float adLoadingInterval = 15f;
        [HorizontalLine] [SerializeField] private EAdNetwork currentNetwork = EAdNetwork.Applovin;


#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        [Header("[admob]")] [SerializeField, Label("Test Mode")]
        private bool admobEnableTestMode;

        [SerializeField, Array, ShowIf(nameof(admobEnableTestMode)), Label("Devices Test")]
        private List<string> admobDevicesTest;

        [SerializeField, Label("Banner")] private AdmobBannerVariable admobBanner;
        [SerializeField, Label("Interstitial")] private AdmobInterVariable admobInter;
        [SerializeField, Label("Rewarded")] private AdmobRewardVariable admobReward;
        [SerializeField, Label("Inter Rewarded")] private AdmobRewardInterVariable admobRewardInter;
        [HorizontalLine] [SerializeField, Label("App Open")] private AdmobAppOpenVariable admobAppOpen;

        public List<string> AdmobDevicesTest => admobDevicesTest;
        public bool AdmobEnableTestMode => admobEnableTestMode;
        public AdmobBannerVariable AdmobBanner => admobBanner;
        public AdmobInterVariable AdmobInter => admobInter;
        public AdmobRewardVariable AdmobReward => admobReward;
        public AdmobRewardInterVariable AdmobRewardInter => admobRewardInter;
        public AdmobAppOpenVariable AdmobAppOpen => admobAppOpen;
#endif


#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        [Header("[applovin]")] [SerializeField, TextArea] private string sdkKey;
        [SerializeField, Label("Banner")] private AdmobBannerVariable applovinBanner;
        [SerializeField, Label("Interstitial")] private AdmobInterVariable applovinInter;
        [SerializeField, Label("Rewarded")] private AdmobRewardVariable applovinReward;
        [SerializeField, Label("Inter Rewarded")] private AdmobRewardInterVariable applovinRewardInter;
        [SerializeField, Label("App Open")] private AdmobAppOpenVariable applovinAppOpen;
        [SerializeField, Label("Age Restricted")] private bool applovinEnableAgeRestrictedUser;
        [SerializeField, Label("Request Ad After Hidden")] private bool applovinEnableRequestAdAfterHidden = true;
        [SerializeField, Label("Max Ad Review")] private bool applovinEnableMaxAdReview;
            
        public bool ApplovinEnableRequestAdAfterHidden => applovinEnableRequestAdAfterHidden;
#endif


        public float AdCheckingInterval => adCheckingInterval;
        public float AdLoadingInterval => adLoadingInterval;
        public EAdNetwork CurrentNetwork { get => currentNetwork; set => currentNetwork = value; }


#if UNITY_EDITOR

        [ShowIf(nameof(IsAdmobSdkNotImported))]
        [SerializeMethod]
        [Color(0,
            0.9f,
            0,
            1,
            Target = ColorTarget.Background)]
        [HorizontalGroup("button-install")]
        private void InstallAdmobSdk()
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            if (!ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group))
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_ADMOB");
                AssetDatabase.Refresh();
            }

            AssetDatabase.ImportPackage(GetPathInCurrentEnvironent("Modules/Apex/ExLib/Core/Editor/Misc/UnityPackages/admob.unitypackage"), false);
        }

        [ShowIf(nameof(IsAdmobSdkImported))]
        [SerializeMethod]
        [Color(1,
            0,
            0,
            0.88f,
            Target = ColorTarget.Background)]
        [HorizontalGroup("button-uninstall")]
        private void UnInstallAdmobSdk()
        {
            
        }

        private bool IsAdmobSdkImported() { return UnityEditor.AssetDatabase.FindAssets("l:gvhp_exportpath-GoogleMobileAds/GoogleMobileAds.dll").Length >= 1; }

        private bool IsAdmobSdkNotImported() => !IsAdmobSdkImported();

        private bool IsInstallAdmob()
        {
            var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            return IsAdmobSdkImported() && ScriptingDefinition.IsSymbolDefined("PANCAKE_ADMOB", group);
        }

        private void InstallApplovinSdk() { }


        private string GetPathInCurrentEnvironent(string fullRelativePath)
        {
            var upmPath = $"Packages/com.pancake.heart/{fullRelativePath}";
            var normalPath = $"Assets/heart/{fullRelativePath}";
            return !System.IO.File.Exists(System.IO.Path.GetFullPath(upmPath)) ? normalPath : upmPath;
        }
#endif
    }
}