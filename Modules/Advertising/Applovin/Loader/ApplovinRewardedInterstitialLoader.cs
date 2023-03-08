namespace Pancake.Monetization
{
    public class ApplovinRewardedInterstitialLoader
    {
        private readonly ApplovinAdClient _client;

        public ApplovinRewardedInterstitialLoader(ApplovinAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
#if PANCAKE_APPLOVIN
            MaxSdkCallbacks.RewardedInterstitial.OnAdClickedEvent += OnAdClicked;
            MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayedEvent += OnAdDisplayed;
            MaxSdkCallbacks.RewardedInterstitial.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayFailedEvent += OnAdDisplayFailed;
            MaxSdkCallbacks.RewardedInterstitial.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.RewardedInterstitial.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.RewardedInterstitial.OnAdReceivedRewardEvent += OnAdReceivedReward;
            MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
#endif
        }

#if PANCAKE_APPLOVIN
        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info)
        {
            _client.InvokeRewardedInterstitialAdRevenuePaid(info);
#if PANCAKE_ANALYTIC
            AppTracking.TrackingRevenue(info);  
#endif
        }

        private void OnAdReceivedReward(string unit, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdReceivedReward(reward); }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { _client.InvokeRewardedInterstitialAdFaildToLoad(); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdLoaded(); }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdFaildToDisplay(); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info)
        {
            R.isShowingAd = false;
            _client.InvokeRewardedInterstitialAdHidden();
            if (AdSettings.MaxSettings.EnableRequestAdAfterHidden) _client.LoadRewardedInterstitialAd();
        }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdDisplay(); }

        private void OnAdClicked(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdClicked(); }
#endif
    }
}