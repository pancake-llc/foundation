#if PANCAKE_ADVERTISING && PANCAKE_APPLOVIN
using System;

namespace Pancake.Monetization
{
    public class ApplovinRewardedLoader : IRewarded
    {
        private readonly ApplovinAdClient _client;

        public ApplovinRewardedLoader(ApplovinAdClient client)
        {
            _client = client;
            Initialized();
        }

        private void Initialized()
        {
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnAdClicked;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnAdDisplayed;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnAdHidden;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnAdDisplayFailed;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnAdLoadFailed;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaid;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnAdReceivedReward;
        }

        private void OnAdReceivedReward(string unit, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdReceivedReward(reward); }

        private void OnAdRevenuePaid(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdRevenuePaid(info); }

        private void OnAdLoadFailed(string unit, MaxSdkBase.ErrorInfo error) { _client.InvokeRewardedAdFaildToLoad(); }

        private void OnAdDisplayFailed(string unit, MaxSdkBase.ErrorInfo error, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdFaildToDisplay(); }

        private void OnAdLoaded(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdLoaded(); }

        private void OnAdHidden(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdHidden(); }

        private void OnAdDisplayed(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdDisplay(); }

        private void OnAdClicked(string unit, MaxSdkBase.AdInfo info) { _client.InvokeRewardedAdClicked(); }

        public void Register(string key, Action action)
        {
            switch (key)
            {
                case "OnDisplayed":
                    _client.rewardedDisplayChain = action;
                    break;
                case "OnCompleted":
                    _client.rewardedCompletedChain = action;
                    break;
                case "OnClosed":
                    _client.rewardedClosedChain = action;
                    break;
                case "OnSkipped":
                    _client.rewardedSkippedChain = action;
                    break;
            }
        }
    }
}
#endif