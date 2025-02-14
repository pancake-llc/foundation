using Pancake.Common;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
using Pancake.Tracking;
#endif

namespace Pancake.Monetization
{
    public sealed class AdmobClient : AdClient
    {
        public override void Init()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            MobileAds.Initialize(_ =>
            {
                App.RunOnMainThread(() =>
                {
                    // Indicates if the Unity app should be paused when a full-screen ad is displayed.
                    // On Android, Unity is paused when displaying full-screen ads. Calling this method with true duplicates this behavior on iOS.
                    MobileAds.SetiOSAppPauseOnBackground(true);
                    if (!AdSettings.AdmobEnableTestMode) return;
                    var configuration = new RequestConfiguration {TestDeviceIds = AdSettings.AdmobDevicesTest};
                    MobileAds.SetRequestConfiguration(configuration);
                });
            });

            AdSettings.AdmobBanner.paidedCallback = AppTracking.TrackRevenue;
            AdSettings.AdmobInter.paidedCallback = AppTracking.TrackRevenue;
            AdSettings.AdmobReward.paidedCallback = AppTracking.TrackRevenue;
            AdSettings.AdmobRewardInter.paidedCallback = AppTracking.TrackRevenue;
            AdSettings.AdmobAppOpen.paidedCallback = AppTracking.TrackRevenue;
            RegisterAppStateChange();
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
            LoadAppOpen();
            LoadBanner();
            (AdSettings.AdmobBanner as IBannerHide)?.Hide(); // hide banner first time when banner auto show when loaded
#endif
        }

        public override void LoadBanner() { AdSettings.AdmobBanner.Load(); }

        public override void LoadInterstitial()
        {
            if (!IsInterstitialReady()) AdSettings.AdmobInter.Load();
        }

        public override bool IsInterstitialReady() { return AdSettings.AdmobInter.IsReady(); }

        public override void LoadRewarded()
        {
            if (!IsRewardedReady()) AdSettings.AdmobReward.Load();
        }

        public override bool IsRewardedReady() { return AdSettings.AdmobReward.IsReady(); }

        public override void LoadRewardedInterstitial()
        {
            if (!IsRewardedInterstitialReady()) AdSettings.AdmobRewardInter.Load();
        }

        public override bool IsRewardedInterstitialReady() { return AdSettings.AdmobRewardInter.IsReady(); }

        private void ShowAppOpen()
        {
            if (statusAppOpenFirstIgnore && !Advertising.isShowingAd) AdSettings.AdmobAppOpen.Show();
            statusAppOpenFirstIgnore = true;
        }

        public override void LoadAppOpen()
        {
            if (!IsAppOpenReady()) AdSettings.AdmobAppOpen.Load();
        }

        public override bool IsAppOpenReady() { return AdSettings.AdmobAppOpen.IsReady(); }

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private void RegisterAppStateChange() { AppStateEventNotifier.AppStateChanged += OnAppStateChanged; }

        private void OnAppStateChanged(GoogleMobileAds.Common.AppState state)
        {
            if (state == GoogleMobileAds.Common.AppState.Foreground)
            {
                if (AdSettings.CurrentNetwork == EAdNetwork.Admob) ShowAppOpen();
            }
        }
#endif
    }
}