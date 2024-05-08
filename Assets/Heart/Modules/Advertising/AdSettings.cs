using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Monetization
{
    [EditorIcon("so_blue_setting")]
    public class AdSettings : ScriptableObject
    {
        [Range(5, 100), SerializeField] private float adCheckingInterval = 8f;
        [Range(5, 100), SerializeField] private float adLoadingInterval = 15f;
        [SerializeField] private EAdNetwork currentNetwork = EAdNetwork.Applovin;
        [SerializeField] private bool gdpr;
        [SerializeField] private bool gdprTestMode;

        [Header("[admob]")] [SerializeField] private bool admobEnableTestMode;

        [SerializeField] private List<string> admobDevicesTest;
        [SerializeField] private AdmobBanner admobBanner = new();
        [SerializeField] private AdmobInter admobInter = new();
        [SerializeField] private AdmobReward admobReward = new();
        [SerializeField] private AdmobRewardInter admobRewardInter = new();
        [SerializeField] private AdmobAppOpen admobAppOpen = new();

        public List<string> AdmobDevicesTest => admobDevicesTest;
        public bool AdmobEnableTestMode => admobEnableTestMode;
        public AdUnit AdmobBanner => admobBanner;
        public AdUnit AdmobInter => admobInter;
        public AdUnit AdmobReward => admobReward;
        public AdUnit AdmobRewardInter => admobRewardInter;
        public AdUnit AdmobAppOpen => admobAppOpen;

        [Header("[applovin]")] [SerializeField, TextArea] private string sdkKey;

        [SerializeField] private ApplovinBanner applovinBanner = new();
        [SerializeField] private ApplovinInter applovinInter = new();
        [SerializeField] private ApplovinReward applovinReward = new();
        [SerializeField] private ApplovinRewardInter applovinRewardInter = new();
        [SerializeField] private ApplovinAppOpen applovinAppOpen = new();
        [SerializeField] private bool applovinEnableAgeRestrictedUser;

        public string SDKKey => sdkKey;
        public AdUnit ApplovinBanner => applovinBanner;
        public AdUnit ApplovinInter => applovinInter;
        public AdUnit ApplovinReward => applovinReward;
        public AdUnit ApplovinRewardInter => applovinRewardInter;
        public AdUnit ApplovinAppOpen => applovinAppOpen;
        public bool ApplovinEnableAgeRestrictedUser => applovinEnableAgeRestrictedUser;
        public float AdCheckingInterval => adCheckingInterval;
        public float AdLoadingInterval => adLoadingInterval;
        public EAdNetwork CurrentNetwork { get => currentNetwork; set => currentNetwork = value; }
        public bool Gdpr => gdpr;
        public bool GdprTestMode => gdprTestMode;
    }
}