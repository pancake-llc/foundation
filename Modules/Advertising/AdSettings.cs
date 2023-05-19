using System.Collections.Generic;
using Pancake.Apex;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pancake.Monetization
{
    [HideMonoScript]
    [EditorIcon("scriptable_ad")]
    public class AdSettings : ScriptableSettings<AdSettings>
    {
        /// <summary>
        /// prevent show app open ad, it will become true when interstitial or rewarded was showed
        /// </summary>
        internal static bool isShowingAd;

        public static bool IsRemoveAd { get => Data.Load($"{Application.identifier}_removeads", false); set => Data.Save($"{Application.identifier}_removeads", value); }

        [Message("Default, ad will be auto loading.")] [Range(5, 100), SerializeField]
        private float adCheckingInterval = 8f;

        [Range(5, 100), SerializeField] private float adLoadingInterval = 15f;
        [SerializeField] private EAdNetwork currentNetwork = EAdNetwork.Applovin;

        [HorizontalLine] [Header("admob")] [SerializeField, Label("Devices Test")]
        private List<string> admobDevicesTest;

        [SerializeField, Label("Banner")] private AdmobBannerVariable admobBanner;
        [SerializeField, Label("Interstitial")] private AdmobInterVariable admobInter;
        [SerializeField, Label("Rewarded")] private AdmobRewardVariable admobReward;

        [FormerlySerializedAs("admobInterReward")] [SerializeField, Label("Inter Rewarded")]
        private AdmobRewardInterVariable admobRewardInter;

        [SerializeField, Label("Open Ad")] private AdmobOpenAdVariable admobOpenAd;


        [HorizontalLine] [Header("applovin")] [SerializeField, TextArea]
        private string sdkKey;

        [SerializeField, Label("Banner")] private AdmobBannerVariable applovinBanner;
        [SerializeField, Label("Interstitial")] private AdmobInterVariable applovinInter;
        [SerializeField, Label("Rewarded")] private AdmobRewardVariable applovinReward;

        [FormerlySerializedAs("applovinInterReward")] [SerializeField, Label("Inter Rewarded")]
        private AdmobRewardInterVariable applovinRewardInter;

        [SerializeField, Label("Open Ad")] private AdmobOpenAdVariable applovinOpenAd;
        [SerializeField, Label("Age Restricted")] private bool applovinEnableAgeRestrictedUser;
        [SerializeField, Label("Request Ad After Hidden")] private bool applovinEnableRequestAdAfterHidden = true;
        [SerializeField, Label("Max Ad Review")] private bool applovinEnableMaxAdReview;


        public float AdCheckingInterval => adCheckingInterval;
        public float AdLoadingInterval => adLoadingInterval;
        public EAdNetwork CurrentNetwork { get => currentNetwork; set => currentNetwork = value; }
        public static List<string> AdmobDevicesTest => Instance.admobDevicesTest;
        public static bool ApplovinEnableRequestAdAfterHidden => Instance.applovinEnableRequestAdAfterHidden;
    }
}