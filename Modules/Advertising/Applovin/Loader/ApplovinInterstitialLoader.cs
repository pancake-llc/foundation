#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
using System;

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
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnAdClicked;
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnAdDisplayed;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnAdDisplayFailed;
        }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdFaildToDisplay(); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdHidden(); }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdDisplay(); }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdRevenuePaid(info); }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { _client.InvokeInterstitialAdFaildToLoad(); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdLoaded(); }

        private void OnAdClicked(string unit, MaxSdkBase.AdInfo info) { _client.InvokeInterstitialAdClicked(); }

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