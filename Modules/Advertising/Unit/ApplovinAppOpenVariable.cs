using System;
using UnityEngine;

// ReSharper disable AccessToStaticMemberViaDerivedType
namespace Pancake.Monetization
{
    [Serializable]
    [EditorIcon("scriptable_variable")]
    public class ApplovinAppOpenVariable : AdUnitVariable
    {
        private bool _registerCallback;

        public override void Load()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            if (AdStatic.IsRemoveAd || string.IsNullOrEmpty(Id)) return;
            if (!_registerCallback)
            {
                MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAdDisplayed;
                MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAdHidden;
                MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAdLoaded;
                MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAdDisplayFailed;
                MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAdLoadFailed;
                MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaid;
                _registerCallback = true;
            }

            MaxSdk.LoadAppOpenAd(Id);
#endif
        }

        public override bool IsReady()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            return !string.IsNullOrEmpty(Id) && MaxSdk.IsAppOpenAdReady(Id);
#else
            return false;
#endif
        }

        protected override void ShowImpl()
        {
#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
            MaxSdk.ShowAppOpenAd(Id);
#endif
        }

        public override void Destroy() { }


#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { C.CallActionClean(ref loadedCallback); }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            paidedCallback?.Invoke(info.Revenue,
                info.NetworkName,
                unit,
                info.AdFormat,
                EAdNetwork.Applovin.ToString());
        }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo info) { C.CallActionClean(ref failedToLoadCallback); }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo info) { C.CallActionClean(ref failedToDisplayCallback); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            AdStatic.isShowingAd = false;
            C.CallActionClean(ref closedCallback);

            if (!string.IsNullOrEmpty(Id)) MaxSdk.LoadAppOpenAd(Id); // ApplovinEnableRequestAdAfterHidden as true
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info)
        {
            AdStatic.isShowingAd = true;
            C.CallActionClean(ref displayedCallback);
        }
#endif
    }
}