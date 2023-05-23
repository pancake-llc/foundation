#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
using Pancake.Tracking;
#endif
using UnityEngine;

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    public class ApplovinAdClient : AdClient
    {
        [field: SerializeField] public override EAdNetwork ClientType { get; } = EAdNetwork.Applovin;


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

        public override AdUnitVariable ShowBanner()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            adSettings.ApplovinBanner.Load();
            return adSettings.ApplovinBanner.Show();
#else
            return null;
#endif
        }

        public override void DestroyBanner()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            adSettings.ApplovinBanner.Destroy();
#endif
        }

        public override AdUnitVariable ShowInterstitial()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return adSettings.ApplovinInter.Show();
#else
            return null;
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

        public override AdUnitVariable ShowRewarded()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return adSettings.ApplovinReward.Show();
#else
            return null;
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

        public override AdUnitVariable ShowRewardedInterstitial()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return adSettings.ApplovinRewardInter.Show();
#else
            return null;
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

        public override AdUnitVariable ShowAppOpen()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return adSettings.ApplovinAppOpen.Show();
#else
            return null;
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