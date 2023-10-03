using System;

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
using GoogleMobileAds.Api;
#endif

namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class AdmobRewardVariable : AdUnitVariable
    {
        [NonSerialized] internal Action completedCallback;
        [NonSerialized] internal Action skippedCallback;
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
        private RewardedAd _rewardedAd;
#endif

        public bool IsEarnRewarded { get; private set; }

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (string.IsNullOrEmpty(Id)) return;
            Destroy();
            RewardedAd.Load(Id, new AdRequest(), AdLoadCallback);
#endif
        }

        public override bool IsReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            return _rewardedAd != null && _rewardedAd.CanShowAd();
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            _rewardedAd.Show(UserRewardEarnedCallback);
#endif
        }

        public override AdUnitVariable Show()
        {
            ResetChainCallback();
            if (!UnityEngine.Application.isMobilePlatform || string.IsNullOrEmpty(Id) || !IsReady()) return this;
            ShowImpl();
            return this;
        }

        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
            skippedCallback = null;
        }

        public override void Destroy()
        {
#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
            if (_rewardedAd == null) return;
            _rewardedAd.Destroy();
            _rewardedAd = null;
            IsEarnRewarded = false;
#endif
        }

#if PANCAKE_ADVERTISING && PANCAKE_ADMOB
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
            _rewardedAd.OnAdPaid += OnAdPaided;
            OnAdLoaded();
        }

        private void OnAdPaided(AdValue value)
        {
            paidedCallback?.Invoke(value.Value / 1000000f,
                "Admob",
                Id,
                "RewardedAd",
                EAdNetwork.Admob.ToString());
        }

        private void OnAdOpening()
        {
            AdStatic.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }

        private void OnAdFailedToShow(AdError obj) { C.CallActionClean(ref failedToDisplayCallback); }

        private void OnAdClosed()
        {
            AdStatic.isShowingAd = false;
            C.CallActionClean(ref closedCallback);
            if (IsEarnRewarded)
            {
                C.CallActionClean(ref completedCallback);
                Destroy();
                return;
            }

            C.CallActionClean(ref skippedCallback);
            Destroy();
        }

        private void OnAdLoaded() { C.CallActionClean(ref loadedCallback); }

        private void OnAdFailedToLoad(LoadAdError error) { C.CallActionClean(ref failedToLoadCallback); }

        private void UserRewardEarnedCallback(Reward reward) { IsEarnRewarded = true; }
#endif

#if UNITY_EDITOR
        [UnityEngine.ContextMenu("Copy Default Test Id")]
        protected void FillDefaultTestId()
        {
#if UNITY_ANDROID
            "ca-app-pub-3940256099942544/5224354917".CopyToClipboard();
#elif UNITY_IOS
            "ca-app-pub-3940256099942544/1712485313".CopyToClipboard();
#endif
            DebugEditor.Toast("[Admob] Copy Rewarded Test Unit Id Success!");
        }
#endif
    }
}