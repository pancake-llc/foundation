namespace Pancake.Monetization
{
    [System.Serializable]
    public class ApplovinRewardedInterstitialUnit : RewardedInterstitialAdUnit
    {
        public ApplovinRewardedInterstitialUnit(string iOSId, string androidId) : base(iOSId, androidId)
        {
        }
    }
}