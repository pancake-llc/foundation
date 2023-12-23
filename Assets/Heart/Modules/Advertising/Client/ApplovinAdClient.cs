#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
using Pancake.Tracking;
using UnityEngine;
#endif

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    [EditorIcon("scriptable_ad")]
    public class ApplovinAdClient : AdClient
    {
        public override void Init()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            MaxSdk.SetSdkKey(adSettings.SDKKey);
            MaxSdk.InitializeSdk();
            MaxSdk.SetIsAgeRestrictedUser(adSettings.ApplovinEnableAgeRestrictedUser);
            adSettings.ApplovinBanner.paidedCallback = AppTracking.TrackRevenue;
            adSettings.ApplovinInter.paidedCallback = AppTracking.TrackRevenue;
            adSettings.ApplovinReward.paidedCallback = AppTracking.TrackRevenue;
            adSettings.ApplovinRewardInter.paidedCallback = AppTracking.TrackRevenue;
            adSettings.ApplovinAppOpen.paidedCallback = AppTracking.TrackRevenue;
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
            LoadAppOpen();
#endif
        }

        public override void LoadInterstitial()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (!IsInterstitialReady()) adSettings.ApplovinInter.Load();
#endif
        }

        public override bool IsInterstitialReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return adSettings.ApplovinInter.IsReady();
#else
            return false;
#endif
        }

        public override void LoadRewarded()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (!IsRewardedReady()) adSettings.ApplovinReward.Load();
#endif
        }

        public override bool IsRewardedReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return adSettings.ApplovinReward.IsReady();
#else
            return false;
#endif
        }

        public override void LoadRewardedInterstitial()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (!IsRewardedInterstitialReady()) adSettings.ApplovinRewardInter.Load();
#endif
        }

        public override bool IsRewardedInterstitialReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return adSettings.ApplovinRewardInter.IsReady();
#else
            return false;
#endif
        }

        internal void ShowAppOpen()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (statusAppOpenFirstIgnore) adSettings.ApplovinAppOpen.Show();
            statusAppOpenFirstIgnore = true;
#endif
        }

        public override void LoadAppOpen()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (!IsAppOpenReady()) adSettings.ApplovinAppOpen.Load();
#endif
        }

        public override bool IsAppOpenReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return adSettings.ApplovinAppOpen.IsReady();
#else
            return false;
#endif
        }
    }
}