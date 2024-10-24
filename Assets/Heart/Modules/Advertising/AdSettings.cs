using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Monetization
{
    [EditorIcon("so_blue_setting")]
    public class AdSettings : ScriptableSettings<AdSettings>
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

        public static List<string> AdmobDevicesTest => Instance.admobDevicesTest;
        public static bool AdmobEnableTestMode => Instance.admobEnableTestMode;
        public static AdUnit AdmobBanner => Instance.admobBanner;
        public static AdUnit AdmobInter => Instance.admobInter;
        public static AdUnit AdmobReward => Instance.admobReward;
        public static AdUnit AdmobRewardInter => Instance.admobRewardInter;
        public static AdUnit AdmobAppOpen => Instance.admobAppOpen;

        [Header("[applovin]")] [SerializeField] private bool enableMaxAdReview;
        [SerializeField] private ApplovinBanner applovinBanner = new();
        [SerializeField] private ApplovinInter applovinInter = new();
        [SerializeField] private ApplovinReward applovinReward = new();
        [SerializeField] private ApplovinRewardInter applovinRewardInter = new();
        [SerializeField] private ApplovinAppOpen applovinAppOpen = new();

        public static AdUnit ApplovinBanner => Instance.applovinBanner;
        public static AdUnit ApplovinInter => Instance.applovinInter;
        public static AdUnit ApplovinReward => Instance.applovinReward;
        public static AdUnit ApplovinRewardInter => Instance.applovinRewardInter;
        public static AdUnit ApplovinAppOpen => Instance.applovinAppOpen;
        public static float AdCheckingInterval => Instance.adCheckingInterval;
        public static float AdLoadingInterval => Instance.adLoadingInterval;
        public static EAdNetwork CurrentNetwork { get => Instance.currentNetwork; set => Instance.currentNetwork = value; }
        public static bool Gdpr => Instance.gdpr;
        public static bool GdprTestMode => Instance.gdprTestMode;
    }
}