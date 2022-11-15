using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class ApplovinRewardedInterstitialUnit : RewardedInterstitialAdUnit
    {
        public ApplovinRewardedInterstitialUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}