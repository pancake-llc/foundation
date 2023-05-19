using System;

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class ApplovinInterVariable : AdUnitVariable
    {
        [NonSerialized] public Action completedCallback;

        private bool _registerCallback;

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (!_registerCallback)
            {
                MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnAdLoaded;
                MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnAdLoadFailed;
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
                MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnAdDisplayed;
                MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnAdHidden;
                MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnAdDisplayFailed;
                _registerCallback = true;
            }

            MaxSdk.LoadInterstitial(Id);
#endif
        }

        public override bool IsReady() { return !string.IsNullOrEmpty(Id) && MaxSdk.IsInterstitialReady(Id); }

        protected override void ShowImpl() { MaxSdk.ShowInterstitial(Id); }

        public override void Destroy() { }

#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { C.CallActionClean(ref faildedToDisplayCallback); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            AdSettings.isShowingAd = false;
            C.CallActionClean(ref completedCallback);
            if (!AdSettings.ApplovinEnableRequestAdAfterHidden || string.IsNullOrEmpty(Id)) MaxSdk.LoadInterstitial(Id);
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info)
        {
            AdSettings.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info) { paidedCallback?.Invoke(info.Revenue, unit, info.NetworkName); }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { C.CallActionClean(ref faildedToLoadCallback); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { C.CallActionClean(ref loadedCallback); }

#endif
    }
}