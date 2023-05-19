using System;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class AdmobRewardInterVariable : AdUnitVariable
    {
        [NonSerialized] public Action completedCallback;
        [NonSerialized] public Action skippedCallback;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private RewardedInterstitialAd _rewardedInterstitialAd;
#endif

        public bool IsEarnRewarded { get; private set; }

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
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
            _rewardedInterstitialAd.OnAdImpressionRecorded += OnAdImpressionRecorded;
            _rewardedInterstitialAd.OnAdFullScreenContentOpened += OnAdOpening;
            _rewardedInterstitialAd.OnAdFullScreenContentFailed += OnAdFailedToShow;
            _rewardedInterstitialAd.OnAdPaid += OnAdPaided;
            OnAdLoaded();
        }

        private void OnAdFailedToLoad(LoadAdError error) { C.CallActionClean(ref faildedToLoadCallback); }

        private void OnAdLoaded() { C.CallActionClean(ref loadedCallback); }

        private void OnAdPaided(AdValue value) { paidedCallback?.Invoke(value.Value / 1000000f, Id, EAdNetwork.Admob.ToString()); }

        private void OnAdFailedToShow(AdError error) { C.CallActionClean(ref faildedToDisplayCallback); }

        private void OnAdOpening()
        {
            AdSettings.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }

        private void OnAdImpressionRecorded() { throw new NotImplementedException(); }

        private void OnAdClosed()
        {
            AdSettings.isShowingAd = false;
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