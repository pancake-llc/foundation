using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Pancake.Monetization
{
    public sealed class AdmobBannerLoader
    {
        private BannerView _bannerView;
        private bool _isLoaded;
        private readonly AdmobAdClient _client;
        private readonly WaitForSeconds _waitReload = new WaitForSeconds(5f);

        public AdmobBannerLoader(AdmobAdClient client)
        {
            _client = client;
            Load();
        }

        public void Load()
        {
            Destroy();
            _bannerView = new BannerView(AdSettings.AdmobSettings.BannerAdUnit.Id,
                AdSettings.AdmobSettings.BannerAdUnit.ConvertSize(),
                AdSettings.AdmobSettings.BannerAdUnit.ConvertPosition());
            _bannerView.OnAdFullScreenContentClosed += OnAdClosed;
            _bannerView.OnBannerAdLoadFailed += OnAdFailedToLoad;
            _bannerView.OnBannerAdLoaded += OnAdLoaded;
            _bannerView.OnAdFullScreenContentOpened += OnAdOpening;
            _bannerView.OnAdPaid += OnAdPaided;
            _bannerView.LoadAd(Admob.CreateRequest());
        }

        private void OnAdPaided(AdValue value) { _client.InvokeBannerAdPaided(value); }

        private void OnAdOpening() { _client.InvokeBannerAdDisplayed(); }

        private void OnAdLoaded() { _client.InvokeBannerAdLoaded(); }

        private void OnAdFailedToLoad(LoadAdError error)
        {
            _client.InvokeBannerAdFailedToLoad(error);
            Runtime.RunCoroutine(DelayReload());
        }

        private IEnumerator DelayReload()
        {
            yield return _waitReload;
            Load();
        }

        private void OnAdClosed() { _client.InvokeBannerAdCompleted(); }
        
        public void Destroy()
        {
            if (_bannerView == null) return;
            _bannerView.Destroy();
            _bannerView = null;
        }

        public void Show() { _bannerView?.Show(); }
    }
}