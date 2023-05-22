#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
using Pancake.Tracking;
#endif

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
#endif
        }

        public override AdUnitVariable ShowBanner()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            adSettings.AdmobBanner.Load();
            return adSettings.AdmobBanner.Show();
#else
            return null;
#endif
        }

        public override void DestroyBanner()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            adSettings.AdmobBanner.Destroy();
#endif
        }

        public override AdUnitVariable ShowInterstitial()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return adSettings.AdmobInter.Show();
#else
            return null;
#endif
        }

        public override void LoadInterstitial()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (!IsInterstitialReady()) adSettings.AdmobInter.Load();
#endif
        }

        public override bool IsInterstitialReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return adSettings.AdmobInter.IsReady();
#else
            return false;
#endif
        }

        public override AdUnitVariable ShowRewarded()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return adSettings.AdmobReward.Show();
#else
            return null;
#endif
        }

        public override void LoadRewarded()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (!IsRewardedReady()) adSettings.AdmobReward.Load();
#endif
        }

        public override bool IsRewardedReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return adSettings.AdmobReward.IsReady();
#else
            return false;
#endif
        }

        public override AdUnitVariable ShowRewardedInterstitial()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return adSettings.AdmobRewardInter.Show();
#else
            return null;
#endif
        }

        public override void LoadRewardedInterstitial()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (!IsRewardedInterstitialReady()) adSettings.AdmobRewardInter.Load();
#endif
        }

        public override bool IsRewardedInterstitialReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return adSettings.AdmobRewardInter.IsReady();
#else
            return false;
#endif
        }

        public override AdUnitVariable ShowAppOpen()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return adSettings.AdmobAppOpen.Show();
#else
            return null;
#endif
        }

        public override void LoadAppOpen()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (!IsAppOpenReady()) adSettings.AdmobAppOpen.Load();
#endif
        }

        public override bool IsAppOpenReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return adSettings.AdmobAppOpen.IsReady();
#else
            return false;
#endif
        }

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private void RegisterAppStateChange()
        {
            GoogleMobileAds.Api.AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
        }

        private void OnAppStateChanged(GoogleMobileAds.Common.AppState state)
        {
            if (state == GoogleMobileAds.Common.AppState.Foreground) ShowAppOpen();
        }
#endif
    }
}