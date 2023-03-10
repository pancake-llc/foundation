using UnityEngine;
#if PANCAKE_ADMOB
using System;
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    public class AdmobRewardedInterstitialLoader
    {
#if PANCAKE_ADMOB
        private RewardedInterstitialAd _rewardedInterstitialAd;
        private AdmobAdClient _client;
        public bool IsEarnRewardedInterstitial { get; private set; }

        public AdmobRewardedInterstitialLoader(AdmobAdClient client)
        {
            _client = client;
            Load();
        }

        public void Load()
        {
            Destroy();
            RewardedInterstitialAd.Load(AdSettings.AdmobSettings.RewardedInterstitialAdUnit.Id, Admob.CreateRequest(), OnAdLoadCallback);
        }

        private void OnAdLoadCallback(RewardedInterstitialAd ad, LoadAdError error)
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                OnAdFailedToLoad(error);
                return;
            }

            _rewardedInterstitialAd = ad;
            _rewardedInterstitialAd.OnAdFullScreenContentClosed += OnAdClosed;
            _rewardedInterstitialAd.OnAdImpressionRecorded += OnAdImpressionRecorded;
            _rewardedInterstitialAd.OnAdFullScreenContentOpened += OnAdOpening;
            _rewardedInterstitialAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _rewardedInterstitialAd.OnAdPaid += OnAdPaided;
            OnAdLoaded();
        }

        private void OnAdPaided(AdValue value) { _client.InvokeRewardedInterAdPaided(value); }
        private void OnAdImpressionRecorded() { _client.InvokeRewardedInterAdImpressionRecorded(); }
        private void OnAdClosed() { _client.InvokeRewardedInterAdClosed(); }
        private void OnAdOpening() { _client.InvokeRewardedInterAdDisplayed(); }
        private void OnAdLoaded() { _client.InvokeRewardedInterAdLoaded(); }
        private void OnAdFailedToLoad(LoadAdError error) { _client.InvokeRewardedInterAdFailedToLoad(error); }
        private void OnAdFailedToShow(AdError error) { _client.InvokeRewardedInterAdFailedToShow(error); }


        internal void Destroy()
        {
            if (_rewardedInterstitialAd == null) return;
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null;
        }
#endif
    }
}