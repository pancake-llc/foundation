#if PANCAKE_MAX_ENABLE
using System;
#endif

namespace Pancake.Monetization
{
    public class ApplovinAppOpenLoader
    {
        private readonly ApplovinAdClient _client;

        public ApplovinAppOpenLoader(ApplovinAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
#if PANCAKE_MAX_ENABLE
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += OnAdClicked;
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += OnAdDisplayed;
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += OnAdDisplayFailed;
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += OnAdRevenuePaid;
#endif
        }

#if PANCAKE_MAX_ENABLE
        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeAppOpenAdLoaded(); }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            _client.InvokeAppOpenAdRevenuePaid(info);
#if ADS_FIREBASE_TRACKING
            AppTracking.TrackingRevenue(info);
#endif
        }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo info) { _client.InvokeAppOpenAdFaildToLoaded(); }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo info) { _client.InvokeAppOpenAdFaildToDisplay(); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            R.isShowingAd = false;
            _client.InvokeAppOpenAdHidden();
            _client.InternalAppOpenAdCompleted(this);
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info)
        {
            R.isShowingAd = true;
            _client.InvokeAppOpenAdDisplay();
        }

        private void OnAdClicked(string unit, MaxSdkBase.AdInfo info) { _client.InvokeAppOpenAdClicked(); }
#endif
    }
}