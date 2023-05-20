using System.Collections.Generic;
using Pancake.Apex;
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
        [SerializeField] private EAdNetwork currentNetwork = EAdNetwork.Applovin;

        [HorizontalLine] [Header("admob")] [SerializeField, Label("Enable Test Mode")] private bool admobEnableTestMode;
        [SerializeField, Label("Devices Test")] private List<string> admobDevicesTest;

        [SerializeField, Label("Banner")] private AdmobBannerVariable admobBanner;
        [SerializeField, Label("Interstitial")] private AdmobInterVariable admobInter;
        [SerializeField, Label("Rewarded")] private AdmobRewardVariable admobReward;
        [SerializeField, Label("Inter Rewarded")] private AdmobRewardInterVariable admobRewardInter;
        [SerializeField, Label("App Open")] private AdmobAppOpenVariable admobAppOpen;


        [HorizontalLine] [Header("applovin")] [SerializeField, TextArea]
        private string sdkKey;

        [SerializeField, Label("Banner")] private AdmobBannerVariable applovinBanner;
        [SerializeField, Label("Interstitial")] private AdmobInterVariable applovinInter;
        [SerializeField, Label("Rewarded")] private AdmobRewardVariable applovinReward;
        [SerializeField, Label("Inter Rewarded")] private AdmobRewardInterVariable applovinRewardInter;
        [SerializeField, Label("App Open")] private AdmobAppOpenVariable applovinAppOpen;
        [SerializeField, Label("Age Restricted")] private bool applovinEnableAgeRestrictedUser;
        [SerializeField, Label("Request Ad After Hidden")] private bool applovinEnableRequestAdAfterHidden = true;
        [SerializeField, Label("Max Ad Review")] private bool applovinEnableMaxAdReview;


        public float AdCheckingInterval => adCheckingInterval;
        public float AdLoadingInterval => adLoadingInterval;
        public EAdNetwork CurrentNetwork { get => currentNetwork; set => currentNetwork = value; }
        public List<string> AdmobDevicesTest => admobDevicesTest;
        public bool AdmobEnableTestMode => admobEnableTestMode;
        public bool ApplovinEnableRequestAdAfterHidden => applovinEnableRequestAdAfterHidden;
        public AdmobBannerVariable AdmobBanner => admobBanner;
        public AdmobInterVariable AdmobInter => admobInter;
        public AdmobRewardVariable AdmobReward => admobReward;
        public AdmobRewardInterVariable AdmobRewardInter => admobRewardInter;
        public AdmobAppOpenVariable AdmobAppOpen => admobAppOpen;
    }
}