using System;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class ApplovinRewardVariable : AdUnitVariable
    {
        [NonSerialized] public Action completedCallback;
        [NonSerialized] public Action skippedCallback;

        private bool _registerCallback;
        public bool IsEarnRewarded { get; private set; }

        public override bool IsReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return MaxSdk.IsRewardedAdReady(Id) && !string.IsNullOrEmpty(Id);
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            MaxSdk.ShowRewardedAd(Id);
#endif
        }

        public override void Destroy() { }

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (!_registerCallback)
            {
                MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnAdDisplayed;
                MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnAdHidden;
                MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnAdLoaded;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnAdDisplayFailed;
                MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnAdLoadFailed;
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnAdReceivedReward;
                _registerCallback = true;
            }

            if (!string.IsNullOrEmpty(Id)) MaxSdk.LoadRewardedAd(Id);
#endif
        }

#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        private void OnAdReceivedReward(string unit, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info) { IsEarnRewarded = true; }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info) { paidedCallback?.Invoke(info.Revenue, unit, info.NetworkName); }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { C.CallActionClean(ref faildedToLoadCallback); }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { C.CallActionClean(ref faildedToDisplayCallback); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { C.CallActionClean(ref loadedCallback); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            AdSettings.isShowingAd = false;
            C.CallActionClean(ref closedCallback);
            if (AdSettings.ApplovinEnableRequestAdAfterHidden && !IsReady()) MaxSdk.LoadRewardedAd(Id);
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
            AdSettings.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }
#endif
    }
}