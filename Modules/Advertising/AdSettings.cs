using System.Collections.Generic;
using Pancake.Apex;
using UnityEngine;


namespace Pancake.Monetization
{
    [HideMonoScript]
    [EditorIcon("scriptable_ad")]
    public class AdSettings : ScriptableObject
    {
        [Range(5, 100), SerializeField] private float adCheckingInterval = 8f;
        [Range(5, 100), SerializeField] private float adLoadingInterval = 15f;
        [SerializeField] private EAdNetwork currentNetwork = EAdNetwork.Applovin;

        [Header("[admob]")] [SerializeField] private bool admobEnableTestMode;

        [SerializeField] private List<string> admobDevicesTest;
        [SerializeField] private AdmobClient admobClient;
        [SerializeField] private AdmobBannerVariable admobBanner;
        [SerializeField] private AdmobInterVariable admobInter;
        [SerializeField] private AdmobRewardVariable admobReward;
        [SerializeField] private AdmobRewardInterVariable admobRewardInter;

        [SerializeField] private AdmobAppOpenVariable admobAppOpen;

        public List<string> AdmobDevicesTest => admobDevicesTest;
        public bool AdmobEnableTestMode => admobEnableTestMode;
        public AdmobClient AdmobClient => admobClient;
        public AdUnitVariable AdmobBanner => admobBanner;
        public AdUnitVariable AdmobInter => admobInter;
        public AdUnitVariable AdmobReward => admobReward;
        public AdUnitVariable AdmobRewardInter => admobRewardInter;
        public AdUnitVariable AdmobAppOpen => admobAppOpen;

        [Header("[applovin]")] [SerializeField, TextArea] private string sdkKey;

        [SerializeField,] private ApplovinAdClient applovinClient;
        [SerializeField] private ApplovinBannerVariable applovinBanner;
        [SerializeField] private ApplovinInterVariable applovinInter;
        [SerializeField] private ApplovinRewardVariable applovinReward;
        [SerializeField] private ApplovinRewardInterVariable applovinRewardInter;
        [SerializeField] private ApplovinAppOpenVariable applovinAppOpen;
        [SerializeField] private bool applovinEnableAgeRestrictedUser;

        public string SDKKey => sdkKey;
        public ApplovinAdClient ApplovinClient => applovinClient;
        public AdUnitVariable ApplovinBanner => applovinBanner;
        public AdUnitVariable ApplovinInter => applovinInter;
        public AdUnitVariable ApplovinReward => applovinReward;
        public AdUnitVariable ApplovinRewardInter => applovinRewardInter;
        public AdUnitVariable ApplovinAppOpen => applovinAppOpen;
        public bool ApplovinEnableAgeRestrictedUser => applovinEnableAgeRestrictedUser;
        public float AdCheckingInterval => adCheckingInterval;
        public float AdLoadingInterval => adLoadingInterval;
        public EAdNetwork CurrentNetwork { get => currentNetwork; set => currentNetwork = value; }
    }
}