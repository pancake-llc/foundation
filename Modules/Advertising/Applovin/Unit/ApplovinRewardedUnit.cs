namespace Pancake.Monetization
{
    [System.Serializable]
    public class ApplovinRewardedUnit : RewardedAdUnit
    {
        public ApplovinRewardedUnit(string iOSId, string androidId) : base(iOSId, androidId)
        {
        }
    }
}