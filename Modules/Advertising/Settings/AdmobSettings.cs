using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobSettings
    {
        [SerializeField] private bool enable;
        [SerializeField] private bool enableTestMode;
        [SerializeField] private List<string> devicesTest;
        [SerializeField] private AdmobBannerUnit bannerAdUnit;
        [SerializeField] private AdmobIntestitialUnit interstitialAdUnit;
        [SerializeField] private AdmobRewardedUnit rewardedAdUnit;
        [SerializeField] private AdmobRewardedInterstitialUnit rewardedInterstitialAdUnit;
        [SerializeField,] private AdmobAppOpenUnit appOpenAdUnit;
        
#if UNITY_EDITOR
        [NonSerialized] internal List<Network> editorListNetwork = new List<Network>();
        [NonSerialized] internal Network editorImportingNetwork;
#endif

        public bool Enable => enable;
        public List<string> DevicesTest => devicesTest;
        public bool EnableTestMode => enableTestMode;
        public AdmobBannerUnit BannerAdUnit => bannerAdUnit;
        public AdmobIntestitialUnit InterstitialAdUnit => interstitialAdUnit;
        public AdmobRewardedUnit RewardedAdUnit => rewardedAdUnit;
        public AdmobRewardedInterstitialUnit RewardedInterstitialAdUnit => rewardedInterstitialAdUnit;
        public AdmobAppOpenUnit AppOpenAdUnit => appOpenAdUnit;
    }
}