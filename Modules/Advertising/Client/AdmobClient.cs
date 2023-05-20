#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif
using Pancake.Tracking;
using UnityEngine;

namespace Pancake.Monetization
{
    public class AdmobClient : AdClient
    {
        [field: SerializeField] public override EAdNetwork ClientType { get; } = EAdNetwork.Admob;

        public override void Init()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            MobileAds.Initialize(_ =>
            {
                App.RunOnMainThread(() =>
                {
                    if (!adSettings.AdmobEnableTestMode) return;
                    var configuration = new RequestConfiguration {TestDeviceIds = adSettings.AdmobDevicesTest};
                    MobileAds.SetRequestConfiguration(configuration);
                });
            });
#endif

            adSettings.AdmobBanner.paidedCallback = AppTracking.TrackRevenue;
            adSettings.AdmobInter.paidedCallback = AppTracking.TrackRevenue;
            adSettings.AdmobReward.paidedCallback = AppTracking.TrackRevenue;
            adSettings.AdmobRewardInter.paidedCallback = AppTracking.TrackRevenue;
            adSettings.AdmobAppOpen.paidedCallback = AppTracking.TrackRevenue;
            RegisterAppStateChange();
            LoadInterstitial();
            LoadRewarded();
            LoadRewardedInterstitial();
            LoadAppOpen();
        }

        public override AdUnitVariable ShowBanner()
        {
            adSettings.AdmobBanner.Load();
            return adSettings.AdmobBanner.Show();
        }

        public override void DestroyBanner() { adSettings.AdmobBanner.Destroy(); }

        public override AdUnitVariable ShowInterstitial() { return adSettings.AdmobInter.Show(); }

        public override void LoadInterstitial()
        {
            if (!IsInterstitialReady()) adSettings.AdmobInter.Load();
        }

        public override bool IsInterstitialReady() { return adSettings.AdmobInter.IsReady(); }

        public override AdUnitVariable ShowRewarded() { return adSettings.AdmobReward.Show(); }

        public override void LoadRewarded()
        {
            if (!IsRewardedReady()) adSettings.AdmobReward.Load();
        }

        public override bool IsRewardedReady() { return adSettings.AdmobReward.IsReady(); }

        public override AdUnitVariable ShowRewardedInterstitial() { return adSettings.AdmobRewardInter.Show(); }

        public override void LoadRewardedInterstitial()
        {
            if (!IsRewardedInterstitialReady()) adSettings.AdmobRewardInter.Load();
        }

        public override bool IsRewardedInterstitialReady() { return adSettings.AdmobRewardInter.IsReady(); }

        public override AdUnitVariable ShowAppOpen() { return adSettings.AdmobAppOpen.Show(); }

        public override void LoadAppOpen()
        {
            if (!IsAppOpenReady()) adSettings.AdmobAppOpen.Load();
        }

        public override bool IsAppOpenReady() { return adSettings.AdmobAppOpen.IsReady(); }

        private void RegisterAppStateChange()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            GoogleMobileAds.Api.AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
#endif
        }

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private void OnAppStateChanged(GoogleMobileAds.Common.AppState state)
        {
            if (state == GoogleMobileAds.Common.AppState.Foreground) ShowAppOpen();
        }
#endif
    }
}