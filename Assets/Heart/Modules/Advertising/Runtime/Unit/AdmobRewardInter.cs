using System;
using Pancake.Common;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobRewardInter : AdUnit
    {
        [NonSerialized] internal Action completedCallback;
        [NonSerialized] internal Action skippedCallback;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private RewardedInterstitialAd _rewardedInterstitialAd;
#endif

        public bool IsEarnRewarded { get; private set; }

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (string.IsNullOrEmpty(Id)) return;
            Destroy();
            RewardedInterstitialAd.Load(Id, new AdRequest(), OnAdLoadCallback);
#endif
        }

        public override bool IsReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return _rewardedInterstitialAd != null && _rewardedInterstitialAd.CanShowAd();
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            _rewardedInterstitialAd.Show(UserEarnedRewardCallback);
#endif
        }

        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
            skippedCallback = null;
        }

        public override AdUnit Show()
        {
            ResetChainCallback();
            if (!UnityEngine.Application.isMobilePlatform || string.IsNullOrEmpty(Id) || !IsReady()) return this;
            ShowImpl();
            return this;
        }

        public override void Destroy()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (_rewardedInterstitialAd == null) return;
            _rewardedInterstitialAd.Destroy();
            _rewardedInterstitialAd = null;
            IsEarnRewarded = false;
#endif
        }

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
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
            _rewardedInterstitialAd.OnAdFullScreenContentOpened += OnAdOpening;
            _rewardedInterstitialAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _rewardedInterstitialAd.OnAdPaid += OnAdPaided;
            OnAdLoaded();
        }

        private void OnAdFailedToLoad(LoadAdError error) { C.CallActionClean(ref failedToLoadCallback); }

        private void OnAdLoaded() { C.CallActionClean(ref loadedCallback); }

        private void OnAdPaided(AdValue value)
        {
            paidedCallback?.Invoke(value.Value / 1000000f,
                "Admob",
                Id,
                "RewardedInterstitialAd",
                EAdNetwork.Admob.ToString());
        }

        private void OnAdFailedToShow(AdError error) { C.CallActionClean(ref failedToDisplayCallback); }

        private void OnAdOpening()
        {
            Advertising.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }

        private void OnAdClosed()
        {
            Advertising.isShowingAd = false;
            C.CallActionClean(ref closedCallback);
            if (IsEarnRewarded)
            {
                C.CallActionClean(ref completedCallback);
                _rewardedInterstitialAd.Destroy();
                return;
            }

            C.CallActionClean(ref skippedCallback);
            _rewardedInterstitialAd.Destroy();
        }

        private void UserEarnedRewardCallback(Reward reward) { IsEarnRewarded = true; }
#endif
    }
}