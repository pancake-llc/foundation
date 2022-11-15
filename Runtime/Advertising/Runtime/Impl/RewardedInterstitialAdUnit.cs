using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class RewardedInterstitialAdUnit : AdUnit
    {
        public RewardedInterstitialAdUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}