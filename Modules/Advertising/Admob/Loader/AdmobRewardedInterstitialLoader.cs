using System;
using GoogleMobileAds.Api;

namespace Pancake.Monetization
{
    public class AdmobRewardedInterstitialLoader : IRewardedInterstitial
    {
        private RewardedInterstitialAd _rewardedInterstitialAd;
        private readonly AdmobAdClient _client;
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

        private void UserEarnedRewardCallback(Reward reward) { }

        private void Destroy()
        {
            if (_rewardedInterstitialAd == null) return;
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null;
        }

        public bool IsReady => _rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd();

        public void Show()
        {
            if (IsReady) _rewardedInterstitialAd.Show(UserEarnedRewardCallback);
        }

        public void Register(string key, Action action)
        {
            switch (key)
            {
                case "OnDisplayed":
                    _client.rewardedInterstitialDisplayChain = action;
                    break;
                case "OnCompleted":
                    _client.rewardedInterstitialCompletedChain = action;
                    break;
            }
        }
    }
}