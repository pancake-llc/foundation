using UnityEngine;

namespace Pancake.Monetization
{
    public class ApplovinRewardedLoader
    {
        private readonly ApplovinAdClient _client;

        public ApplovinRewardedLoader(ApplovinAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
#if PANCAKE_MAX_ENABLE
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnAdClicked;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnAdDisplayed;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnAdDisplayFailed;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnAdReceivedReward;
#endif
        }

#if PANCAKE_MAX_ENABLE
        private void OnAdReceivedReward(string unit, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdReceivedReward(reward); }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            _client.InvokeRewardedAdRevenuePaid(info);
#if ADS_FIREBASE_TRACKING
            AppTracking.TrackingRevenue(info);  
#endif
        }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { _client.InvokeRewardedAdFaildToLoad(); }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdFaildToDisplay(); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdLoaded(); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            R.isShowingAd = false;
            _client.InvokeRewardedAdHidden();
            if (Settings.MaxSettings.EnableRequestAdAfterHidden) _client.LoadRewardedAd();
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdDisplay(); }

        private void OnAdClicked(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdClicked(); }
#endif
    }
}