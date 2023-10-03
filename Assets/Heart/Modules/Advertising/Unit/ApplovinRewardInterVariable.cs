using System;

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class ApplovinRewardInterVariable : AdUnitVariable
    {
        [NonSerialized] internal Action completedCallback;
        [NonSerialized] internal Action skippedCallback;

        private bool _registerCallback;
        public bool IsEarnRewarded { get; private set; }

        public override bool IsReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return !string.IsNullOrEmpty(Id) && MaxSdk.IsRewardedInterstitialAdReady(Id);
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            MaxSdk.ShowRewardedInterstitialAd(Id);
#endif
        }

        protected override void ResetChainCallback()
        {
            base.ResetChainCallback();
            completedCallback = null;
            skippedCallback = null;
        }

        public override AdUnitVariable Show()
        {
            ResetChainCallback();
            if (!UnityEngine.Application.isMobilePlatform || !IsReady()) return this;
            ShowImpl();
            return this;
        }

        public override void Destroy() { }

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (string.IsNullOrEmpty(Id)) return;
            if (!_registerCallback)
            {
                MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayedEvent += OnAdDisplayed;
                MaxSdkCallbacks.RewardedInterstitial.OnAdHiddenEvent += OnAdHidden;
                MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayFailedEvent += OnAdDisplayFailed;
                MaxSdkCallbacks.RewardedInterstitial.OnAdLoadedEvent += OnAdLoaded;
                MaxSdkCallbacks.RewardedInterstitial.OnAdLoadFailedEvent += OnAdLoadFailed;
                MaxSdkCallbacks.RewardedInterstitial.OnAdReceivedRewardEvent += OnAdReceivedReward;
                MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
                _registerCallback = true;
            }

            MaxSdk.LoadRewardedInterstitialAd(Id);
#endif
        }


#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            paidedCallback?.Invoke(info.Revenue,
                info.NetworkName,
                unit,
                info.AdFormat,
                EAdNetwork.Applovin.ToString());
        }

        private void OnAdReceivedReward(string unit, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info) { IsEarnRewarded = true; }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { C.CallActionClean(ref failedToLoadCallback); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { C.CallActionClean(ref loadedCallback); }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { C.CallActionClean(ref failedToDisplayCallback); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            AdStatic.isShowingAd = false;
            C.CallActionClean(ref closedCallback);
            if (!IsReady()) MaxSdk.LoadRewardedInterstitialAd(Id); // ApplovinEnableRequestAdAfterHidden as true

            if (IsEarnRewarded)
            {
                C.CallActionClean(ref completedCallback);
                IsEarnRewarded = false;
                return;
            }

            C.CallActionClean(ref skippedCallback);
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info)
        {
            AdStatic.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }
#endif
    }
}