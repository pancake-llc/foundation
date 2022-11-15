using System;
using UnityEngine;
using System.Collections.Generic;

namespace Pancake.Monetization
{
    [Serializable]
    public class IronSourceSettings
    {
        [SerializeField] private bool enable;
        [SerializeField] private AppUnit appKey;
        [SerializeField] private bool useAdaptiveBanner;
        [SerializeField] private IronSourceBannerUnit bannerAdUnit;
        [SerializeField] private IronSourceAppOpenUnit appOpenAdUnit;


#if UNITY_EDITOR

        /// <summary>
        /// editor only
        /// </summary>
        public Network editorImportingSdk;

        /// <summary>
        /// editor only
        /// </summary>
        public AdapterMediationIronSource editorImportingNetwork;

        /// <summary>
        /// editor only
        /// </summary>
        public List<AdapterMediationIronSource> editorListNetwork = new List<AdapterMediationIronSource>();
#endif

        public bool Enable => enable;

        public IronSourceBannerUnit BannerAdUnit => bannerAdUnit;

        public AppUnit AppKey => appKey;

        public bool UseAdaptiveBanner => useAdaptiveBanner;
#if PANCAKE_IRONSOURCE_ENABLE && PANCAKE_ADMOB_ENABLE
        public IronSourceAppOpenUnit AppOpenAdUnit => appOpenAdUnit;
#endif
    }
}