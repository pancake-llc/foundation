using System;
using GoogleMobileAds.Api;

namespace Pancake.Monetization
{
    public class AdmobInterstitialLoader : IInterstitial
    {
        private readonly AdmobAdClient _client;
        private InterstitialAd _interstitialAd;

        public AdmobInterstitialLoader(AdmobAdClient client)
        {
            _client = client;
            Load();
        }

        public void Load()
        {
            Destroy();
            InterstitialAd.Load(AdSettings.AdmobSettings.InterstitialAdUnit.Id, Admob.CreateRequest(), AdLoadCallback);
        }

        private void AdLoadCallback(InterstitialAd ad, LoadAdError error)
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                OnAdFailedToLoad(error);
                return;
            }

            _interstitialAd = ad;
            _interstitialAd.OnAdPaid += OnAdPaided;
            _interstitialAd.OnAdFullScreenContentClosed += OnAdClosed;
            _interstitialAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _interstitialAd.OnAdFullScreenContentOpened += OnAdOpening;
            _interstitialAd.OnAdImpressionRecorded += OnAdImpressionRecorded;
            OnAdLoaded();
        }

        private void OnAdImpressionRecorded() { _client.InvokeInterAdImpressionRecorded(); }
        private void OnAdFailedToShow(AdError error) { _client.InvokeInterAdFailedToShow(error); }
        private void OnAdFailedToLoad(LoadAdError error) { _client.InvokeInterAdFailedToLoad(error); }
        private void OnAdLoaded() { _client.InvokeInterAdLoaded(); }
        private void OnAdOpening() { _client.InvokeInterAdDisplayed(); }

        private void OnAdClosed()
        {
            _client.InvokeInterAdCompleted();
            Destroy();
        }

        private void OnAdPaided(AdValue value) { _client.InvokeInterAdPaided(value); }

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

        internal void Destroy()
        {
            if (_interstitialAd == null) return;
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        public bool IsReady => _interstitialAd != null && _interstitialAd.CanShowAd();

        public void Show()
        {
            if (IsReady) _interstitialAd.Show();
        }
    }
}