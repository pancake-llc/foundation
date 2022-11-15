using System;
using UnityEngine;

namespace Pancake.Monetization
{
    [Serializable]
    public class AdSettings
    {
        [SerializeField] private bool autoInit = true;
        [SerializeField] private EAutoLoadingAd autoLoadingAd = EAutoLoadingAd.All;
        [Range(5, 100)] [SerializeField] private float adCheckingInterval = 8f;
        [Range(5, 100)] [SerializeField] private float adLoadingInterval = 15f;
        [SerializeField] private string privacyPolicyUrl;
        [SerializeField] private bool enableGDPR;
        [SerializeField] private bool enableMultipleDex;
        [SerializeField] private EAdNetwork currentNetwork = EAdNetwork.Admob;

        public bool AutoInit { get => autoInit; set => autoInit = value; }

        public float AdCheckingInterval { get => adCheckingInterval; set => adCheckingInterval = value; }

        public float AdLoadingInterval { get => adLoadingInterval; set => adLoadingInterval = value; }

        public EAutoLoadingAd AutoLoadingAd { get => autoLoadingAd; set => autoLoadingAd = value; }

        public string PrivacyPolicyUrl => privacyPolicyUrl;

        public bool EnableGDPR { get => enableGDPR; set => enableGDPR = value; }

        public bool EnableMultipleDex => enableMultipleDex;

        public EAdNetwork CurrentNetwork { get => currentNetwork; set => currentNetwork = value; }
    }
}