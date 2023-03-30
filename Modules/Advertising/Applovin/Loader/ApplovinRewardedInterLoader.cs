#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
using System;

namespace Pancake.Monetization
{
    public class ApplovinRewardedInterLoader : IRewardedInterstitial
    {
        private readonly ApplovinAdClient _client;

        public ApplovinRewardedInterLoader(ApplovinAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
            MaxSdkCallbacks.RewardedInterstitial.OnAdClickedEvent += OnAdClicked;
            MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayedEvent += OnAdDisplayed;
            MaxSdkCallbacks.RewardedInterstitial.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.RewardedInterstitial.OnAdDisplayFailedEvent += OnAdDisplayFailed;
            MaxSdkCallbacks.RewardedInterstitial.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.RewardedInterstitial.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.RewardedInterstitial.OnAdReceivedRewardEvent += OnAdReceivedReward;
            MaxSdkCallbacks.RewardedInterstitial.OnAdRevenuePaidEvent += OnAdRevenuePaid;
        }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdRevenuePaid(info); }

        private void OnAdReceivedReward(string unit, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdReceivedReward(reward); }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { _client.InvokeRewardedInterstitialAdFaildToLoad(); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdLoaded(); }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdFaildToDisplay(); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdHidden(); }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdDisplay(); }

        private void OnAdClicked(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedInterstitialAdClicked(); }

        public void Register(string key, Action action)
        {
            switch (key)
            {
                case "OnDisplayed":
                    _client.rewardedInterDisplayChain = action;
                    break;
                case "OnCompleted":
                    _client.rewardedInterCompletedChain = action;
                    break;
                case "OnClosed":
                    _client.rewardedInterClosedChain = action;
                    break;
                case "OnSkipped":
                    _client.rewardedInterSkippedChain = action;
                    break;
            }
        }
    }
}
#endif