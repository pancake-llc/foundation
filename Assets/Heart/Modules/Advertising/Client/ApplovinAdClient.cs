#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
using Pancake.Tracking;
#endif

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    public sealed class ApplovinAdClient : AdClient
    {
        public override void Init()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            MaxSdk.InitializeSdk();
            AdSettings.ApplovinBanner.paidedCallback = AppTracking.TrackRevenue;
            AdSettings.ApplovinInter.paidedCallback = AppTracking.TrackRevenue;
            AdSettings.ApplovinReward.paidedCallback = AppTracking.TrackRevenue;
            AdSettings.ApplovinRewardInter.paidedCallback = AppTracking.TrackRevenue;
            AdSettings.ApplovinAppOpen.paidedCallback = AppTracking.TrackRevenue;
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
            LoadAppOpen();
            LoadBanner();
            (AdSettings.ApplovinBanner as IBannerHide)?.Hide(); // hide banner first time when banner auto show when loaded
#endif
        }

        public override void LoadBanner() { AdSettings.ApplovinBanner.Load(); }

        public override void LoadInterstitial()
        {
            if (!IsInterstitialReady()) AdSettings.ApplovinInter.Load();
        }

        public override bool IsInterstitialReady() { return AdSettings.ApplovinInter.IsReady(); }

        public override void LoadRewarded()
        {
            if (!IsRewardedReady()) AdSettings.ApplovinReward.Load();
        }

        public override bool IsRewardedReady() { return AdSettings.ApplovinReward.IsReady(); }

        public override void LoadRewardedInterstitial()
        {
            if (!IsRewardedInterstitialReady()) AdSettings.ApplovinRewardInter.Load();
        }

        public override bool IsRewardedInterstitialReady() { return AdSettings.ApplovinRewardInter.IsReady(); }

        internal void ShowAppOpen()
        {
            if (statusAppOpenFirstIgnore && !Advertising.isShowingAd) AdSettings.ApplovinAppOpen.Show();
            statusAppOpenFirstIgnore = true;
        }

        public override void LoadAppOpen()
        {
            if (!IsAppOpenReady()) AdSettings.ApplovinAppOpen.Load();
        }

        public override bool IsAppOpenReady() { return AdSettings.ApplovinAppOpen.IsReady(); }
    }
}
