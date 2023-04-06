#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
#if PANCAKE_ADMOB
using System;
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    public class AdmobAppOpenLoader
    {
        private AppOpenAd _appOpenAd;
        private readonly AdmobAdClient _client;
        private DateTime _expireTime;

        public AdmobAppOpenLoader(AdmobAdClient client)
        {
            _client = client;
            Load();
        }

        public void Load()
        {
            Destroy();
            AppOpenAd.Load(AdSettings.AdmobSettings.AppOpenAdUnit.Id, AdSettings.AdmobSettings.AppOpenAdUnit.orientation, Admob.CreateRequest(), OnAdLoadCallback);
        }

        private void OnAdLoadCallback(AppOpenAd ad, LoadAdError error)
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                OnAdFailedToLoad(error);
                return;
            }

            _appOpenAd = ad;
            _appOpenAd.OnAdPaid += OnAdPaided;
            _appOpenAd.OnAdFullScreenContentClosed += OnAdClosed;
            _appOpenAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _appOpenAd.OnAdFullScreenContentOpened += OnAdOpening;
            _appOpenAd.OnAdImpressionRecorded += OnAdImpressionRecorded;
            OnAdLoaded();

            // App open ads can be preloaded for up to 4 hours.
            _expireTime = DateTime.Now + TimeSpan.FromHours(4);
        }

        private void OnAdImpressionRecorded() { _client.InvokeAppOpenAdImpressionRecorded(); }

        private void OnAdOpening() { _client.InvokeAppOpenAdDisplayed(); }

        private void OnAdFailedToShow(AdError error) { _client.InvokeAppOpenAdFailedToShow(error); }

        private void OnAdClosed()
        {
            _client.InvokeAppOpenAdCompleted();
            Destroy();
        }

        private void OnAdPaided(AdValue value) { _client.InvokeAppOpenAdPaided(value); }

        private void OnAdFailedToLoad(LoadAdError error) { _client.InvokeAppOpenAdFailedToLoad(error); }

        private void OnAdLoaded() { _client.InvokeAppOpenAdLoaded(); }

        private void Destroy()
        {
            if (_appOpenAd == null) return;
            _appOpenAd.Destroy();
            _appOpenAd = null;
        }

        public bool IsReady => _appOpenAd != null && _appOpenAd.CanShowAd() && DateTime.Now < _expireTime;

        public void Show()
        {
            if (IsReady) _appOpenAd.Show();
        }
    }
}
#endif