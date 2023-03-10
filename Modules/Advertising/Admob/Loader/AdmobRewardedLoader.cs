using System;
#if PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    public class AdmobRewardedLoader : IRewarded
    {
        private RewardedAd _rewardedAd;
        private readonly AdmobAdClient _client;
        public bool IsEarnRewarded { get; private set; }


        public AdmobRewardedLoader(AdmobAdClient client)
        {
            _client = client;
            Initialized();
        }


        private void Initialized() { RewardedAd.Load(AdSettings.AdmobSettings.RewardedAdUnit.Id, Admob.CreateRequest(), AdLoadCallback); }

        private void AdLoadCallback(RewardedAd ad, LoadAdError error)
        {
            // if error is not null, the load request failed.
            if (error != null || ad == null)
            {
                OnAdFailedToLoad(error);
                return;
            }

            _rewardedAd = ad;
            _rewardedAd.OnAdFullScreenContentClosed += OnAdClosed;
            _rewardedAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _rewardedAd.OnAdFullScreenContentOpened += OnAdOpening;
            _rewardedAd.OnAdImpressionRecorded += OnAdImpressionRecorded;
            _rewardedAd.OnAdPaid += OnAdPaided;
            OnAdLoaded();
        }

        private void OnAdPaided(AdValue value) { _client.InvokeRewardAdPaided(value); }
        private void OnAdImpressionRecorded() { _client.InvokeRewardAdImpressionRecorded(); }
        private void OnAdOpening() { _client.InvokeRewardAdDisplayed(); }
        private void OnAdFailedToShow(AdError error) { _client.InvokeRewardAdFailedToShow(error); }
        private void OnAdFailedToLoad(LoadAdError error) { _client.InvokeRewardAdFailedToLoad(error); }
        private void OnAdLoaded() { _client.InvokeRewardAdLoaded(); }
        private void OnAdClosed() { _client.InvokeRewardAdCompleted(); }

        public void Register(string key, Action action)
        {
            switch (key)
            {
                case "OnDisplayed":
                    _client.rewardedDisplayChain = action;
                    break;
                case "OnCompleted":
                    _client.rewardedCompletedChain = action;
                    break;
                case "OnClosed":
                    _client.rewardedClosedChain = action;
                    break;
                case "OnSkipped":
                    _client.rewardedSkippedChain = action;
                    break;
            }
        }
    }
}