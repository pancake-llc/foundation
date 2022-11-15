using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobRewardedUnit : RewardedAdUnit
    {
        public AdmobRewardedUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}