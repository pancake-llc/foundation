using UnityEngine;

namespace Pancake.Monetization
{
    public class ApplovinInterstitialLoader
    {
        private readonly ApplovinAdClient _client;

        public ApplovinInterstitialLoader(ApplovinAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
#if PANCAKE_MAX_ENABLE
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnAdClicked;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnAdDisplayed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnAdDisplayFailed;
#endif
        }

#if PANCAKE_MAX_ENABLE
        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdFaildToDisplay(); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            R.isShowingAd = false;
            _client.InvokeInterstitialAdHidden();
            if (Settings.MaxSettings.EnableRequestAdAfterHidden) _client.LoadInterstitialAd();
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdDisplay(); }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            _client.InvokeInterstitialAdRevenuePaid(info);
#if ADS_FIREBASE_TRACKING
            AppTracking.TrackingRevenue(info);  
#endif
        }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { _client.InvokeInterstitialAdFaildToLoad(); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdLoaded(); }

        private void OnAdClicked(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdClicked(); }
#endif
    }
}