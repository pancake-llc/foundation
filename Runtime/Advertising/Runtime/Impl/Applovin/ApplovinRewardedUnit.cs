using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class ApplovinRewardedUnit : RewardedAdUnit
    {
        public ApplovinRewardedUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}