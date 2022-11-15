

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    public class ApplovinBannerLoader
    {
        private readonly ApplovinAdClient _client;

        public ApplovinBannerLoader(ApplovinAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
#if PANCAKE_MAX_ENABLE
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnAdClicked;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnAdExpanded;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnAdCollapsed;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdk.CreateBanner(Settings.MaxSettings.BannerAdUnit.Id, Settings.MaxSettings.BannerAdUnit.ConvertPosition());
            if (Settings.MaxSettings.BannerAdUnit.useAdaptiveBanner)
            {
                MaxSdk.SetBannerExtraParameter(Settings.MaxSettings.BannerAdUnit.Id, "adaptive_banner", "true");
            }
#endif
        }

#if PANCAKE_MAX_ENABLE
        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            _client.InvokeBannerAdRevenuePaid(info);
#if ADS_FIREBASE_TRACKING
            AppTracking.TrackingRevenue(info);  
#endif
        }

        private void OnAdCollapsed(string unit, MaxSdkBase.AdInfo info) { _client.InvokeBannerAdCollapsed(); }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { _client.InvokeBannerAdFaildToLoad(); }

        private void OnAdExpanded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeBannerAdExpanded(); }

        private void OnAdClicked(string unit, MaxSdkBase.AdInfo info) { _client.InvokeBannerAdClicked(); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeBannerAdLoaded(); }

        internal float GetAdaptiveBannerHeight() { return MaxSdkUtils.GetAdaptiveBannerHeight(UnityEngine.Screen.width); }
#endif
    }
}