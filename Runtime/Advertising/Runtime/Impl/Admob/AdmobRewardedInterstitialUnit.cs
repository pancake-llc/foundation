using System;

namespace Pancake.Monetization
{
    [Serializable]
    public class AdmobRewardedInterstitialUnit : RewardedInterstitialAdUnit
    {
        public AdmobRewardedInterstitialUnit(string iOSId, string androidId)
            : base(iOSId, androidId)
        {
        }
    }
}