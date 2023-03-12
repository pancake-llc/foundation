using System;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    public class AdCommonSettings
    {
        [SerializeField] private bool autoInit = true;
        [SerializeField] private EAutoLoadingAd autoLoadingAd = EAutoLoadingAd.All;
        [Range(5, 100), SerializeField] private float adCheckingInterval = 8f;
        [Range(5, 100), SerializeField] private float adLoadingInterval = 15f;
        [SerializeField] private bool enableGdpr;
        [SerializeField] private string privacyUrl;
        [SerializeField] private bool multiDex;
        [SerializeField] private bool hideAppOpenAdStartup;
        [SerializeField] private EAdNetwork currentNetwork = EAdNetwork.Applovin;

        public bool AutoInit => autoInit;
        public float AdCheckingInterval => adCheckingInterval;
        public float AdLoadingInterval => adLoadingInterval;
        public string PrivacyUrl => privacyUrl;
        public bool EnableGdpr => enableGdpr;
        public bool MultiDex => multiDex;
        public bool HideAppOpenAdWhenStartup => hideAppOpenAdStartup;

        public EAdNetwork CurrentNetwork { get => currentNetwork; set => currentNetwork = value; }
        public EAutoLoadingAd AutoLoadingAd { get => autoLoadingAd; set => autoLoadingAd = value; }
    }
}