#if PANCAKE_ADS
using System;
using UnityEngine;

namespace Pancake.Monetization
{
    public class ApplovinInterstitialLoader : IInterstitial
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
            if (AdSettings.MaxSettings.EnableRequestAdAfterHidden) _client.LoadInterstitialAd();
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info) { _client.HandleInterstitialAdDisplay(); }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            _client.InvokeInterstitialAdRevenuePaid(info);
#if PANCAKE_ANALYTIC
            AppTracking.TrackingRevenue(info);  
#endif
        }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { _client.InvokeInterstitialAdFaildToLoad(); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdLoaded(); }

        private void OnAdClicked(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdClicked(); }
#endif
        public void Register(string key, Action action)
        {
            switch (key)
            {
                case "OnDisplayed":
                    _client.interstitialDisplayChain = action;
                    break;
                case "OnCompleted":
                    _client.interstitialCompletedChain = action;
                    break;
            }
        }
    }
}
#endif