using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobSettings
    {
        [SerializeField] private bool enable;
        [SerializeField] private List<string> devicesTest;
        [SerializeField] private AdmobBannerUnit bannerAdUnit;
        [SerializeField] private AdmobIntestitialUnit interstitialAdUnit;
        [SerializeField] private AdmobRewardedUnit rewardedAdUnit;
        [SerializeField] private AdmobRewardedInterstitialUnit rewardedInterstitialAdUnit;
        [SerializeField] private AdmobAppOpenUnit appOpenAdUnit;
        [SerializeField] private bool enableTestMode;
        [SerializeField] private bool useAdaptiveBanner;

#if UNITY_EDITOR
        public List<Network> editorListNetwork = new List<Network>();

        /// <summary>
        /// editor only
        /// </summary>
        public Network editorImportingNetwork;

        /// <summary>
        /// editor only
        /// </summary>
        public Network editorImportingSdk;

        /// <summary>
        /// editor only
        /// </summary>
        public List<Network> editorImportingListNetwork = new List<Network>();

        /// <summary>
        /// editor only
        /// </summary>
        public bool editorInstallAllFlag;
#endif

        public bool Enable => enable;
        public List<string> DevicesTest => devicesTest;
        public bool EnableTestMode => enableTestMode;
        public bool UseAdaptiveBanner => useAdaptiveBanner;
        public AdmobBannerUnit BannerAdUnit => bannerAdUnit;
        public AdmobIntestitialUnit InterstitialAdUnit => interstitialAdUnit;
        public AdmobRewardedUnit RewardedAdUnit => rewardedAdUnit;
        public AdmobRewardedInterstitialUnit RewardedInterstitialAdUnit => rewardedInterstitialAdUnit;
        public AdmobAppOpenUnit AppOpenAdUnit => appOpenAdUnit;
    }
}